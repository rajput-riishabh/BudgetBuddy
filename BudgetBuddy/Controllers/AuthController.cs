using Microsoft.AspNetCore.Mvc;
using BudgetBuddy.Models;
using BudgetBuddy.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using BCrypt.Net;
using System;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using BudgetBuddy.Models.ViewModels;
using BudgetBuddy.Services;
using System;
using System.Linq;
using BudgetBuddy.Models.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;



namespace BudgetBuddy.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly JwtConfig _jwtConfig;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IOptions<JwtConfig> jwtConfig, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _jwtConfig = jwtConfig.Value;
            _emailService = emailService;
            _configuration = configuration;
        }

        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model) // Make Login action async
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName); // Use async version
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // 1. Create Claims for the authenticated user (similar to what you did for JWT)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            };

            // 2. Create ClaimsIdentity
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 3. Perform SignInAsync to create and set the authentication cookie
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true, // Optional: Allow cookie to be refreshed
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1), // Set cookie expiration (match Program.cs config)
                //IsPersistent = model.RememberMe // Optional: "Remember Me" functionality
            };

            await HttpContext.SignInAsync(
                 "BudgetBuddyAuth",
                     new ClaimsPrincipal(claimsIdentity),
                  authProperties);

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(); // Use async version

            return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already registered");
                return View(model);
            }

            if (_context.Users.Any(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Username is already taken");
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Create a default budget for each category for the new user
            var categories = _context.Categories.ToList();
            foreach (var category in categories)
            {
                var defaultBudget = new Budget
                {
                    UserId = user.UserId,
                    CategoryId = category.CategoryId,
                    Amount = 0, // Default budget amount
                    StartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
                    EndDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddDays(-1)
                };
                _context.Budgets.Add(defaultBudget);
            }
            _context.SaveChanges();

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> Logout() // Make Logout action async
        {
            await HttpContext.SignOutAsync("BudgetBuddyAuth");
            return RedirectToAction("Index", "Home");
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.GivenName, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes),
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Generate reset token
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            // Save reset token
            var passwordReset = new PasswordReset
            {
                UserId = user.UserId,
                Token = token,
                ExpiryDate = DateTime.Parse("2025-03-11 14:28:48").AddHours(24),
                IsUsed = false
            };

            _context.PasswordResets.Add(passwordReset);
            await _context.SaveChangesAsync();

            // Generate reset link
            var resetLink = Url.Action(
                "ResetPassword",
                "Auth",
                new { email = model.Email, token },
                protocol: Request.Scheme);

            // Send email
            await _emailService.SendPasswordResetEmailAsync(model.Email, resetLink);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var passwordReset = await _context.PasswordResets
                .FirstOrDefaultAsync(r =>
                    r.UserId == user.UserId &&
                    r.Token == model.Token &&
                    r.ExpiryDate > DateTime.Parse("2025-03-11 14:28:48") &&
                    !r.IsUsed);

            if (passwordReset == null)
            {
                ModelState.AddModelError("", "Invalid or expired password reset token.");
                return View(model);
            }

            // Update password
            user.PasswordHash = HashPassword(model.Password);

            // Mark token as used
            passwordReset.IsUsed = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}