using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BudgetBuddy.Models;
using BudgetBuddy.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using BudgetBuddy.Models.Enums;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BudgetBuddy.Services;
using Microsoft.AspNetCore.Hosting;

namespace BudgetBuddy.Controllers
{
    [Authorize(AuthenticationSchemes = "BudgetBuddyAuth")] // Specify the authentication scheme
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(
            AppDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        [Authorize(Roles = "Admin", AuthenticationSchemes = "BudgetBuddyAuth")]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Select(u => new UserManagementViewModel
                {
                    Id = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    IsActive = u.IsActive,
                    ExpenseCount = u.Expenses.Count,
                    TotalExpenses = u.TotalExpenses
                })
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Profile()
        {
            int userId = 0; // Initialize userId with a default value
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out userId))
            {
                return Unauthorized(); // Or RedirectToAction("Login", "Auth");
            }

            var applicationUser = await _context.Users
                .Include(u => u.Expenses)
                .Include(u => u.Budgets)
                .Include(u => u.CreatedCategories)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (applicationUser == null)
            {
                return NotFound();
            }

            var viewModel = new UserProfileViewModel
            {
                Id = applicationUser.UserId,
                FirstName = applicationUser.FirstName,
                LastName = applicationUser.LastName,
                Email = applicationUser.Email,
                ProfilePicture = applicationUser.ProfilePicture,
                PreferredCurrency = applicationUser.PreferredCurrency,
                TimeZone = applicationUser.TimeZone,
                TotalExpenses = applicationUser.Expenses.Count,
                TotalBudgets = applicationUser.Budgets.Count,
                CustomCategories = applicationUser.CreatedCategories.Count,
                CurrentMonthSpending = applicationUser.CurrentMonthExpenses
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model, IFormFile profilePicture)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(); // Or RedirectToAction("Login", "Auth");
                }

                var applicationUser = await _context.Users.FindAsync(userId);
                if (applicationUser == null)
                {
                    return NotFound();
                }

                applicationUser.FirstName = model.FirstName;
                applicationUser.LastName = model.LastName;
                applicationUser.PreferredCurrency = model.PreferredCurrency;
                applicationUser.TimeZone = model.TimeZone;

                if (profilePicture != null)
                {
                    var fileName = $"{applicationUser.UserId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(profilePicture.FileName)}";
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(stream);
                    }

                    applicationUser.ProfilePicture = $"/uploads/profiles/{fileName}";
                }

                _context.Users.Update(applicationUser);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Profile));
            }

            return View(model);
        }

        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var applicationUser = await _context.Users.FindAsync(userId);
                if (applicationUser == null)
                {
                    return NotFound();
                }

                // Verify the current password
                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, applicationUser.PasswordHash))
                {
                    ModelState.AddModelError("CurrentPassword", "Incorrect current password.");
                    return View(model);
                }

                // Hash the new password
                applicationUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                _context.Users.Update(applicationUser);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction(nameof(Profile));
            }

            return View(model);
        }

        [Authorize(Roles = "Admin", AuthenticationSchemes = "BudgetBuddyAuth")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User status updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}