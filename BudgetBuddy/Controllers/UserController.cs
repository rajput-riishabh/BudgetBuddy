using Microsoft.AspNetCore.Identity;
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

using BudgetBuddy.Services;

namespace BudgetBuddy.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<int>> roleManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePicture = user.ProfilePicture,
                PreferredCurrency = user.PreferredCurrency,
                TimeZone = user.TimeZone,
                TotalExpenses = user.Expenses.Count,
                TotalBudgets = user.Budgets.Count,
                CustomCategories = user.CreatedCategories.Count,
                CurrentMonthSpending = user.CurrentMonthExpenses
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model, IFormFile profilePicture)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.PreferredCurrency = model.PreferredCurrency;
                user.TimeZone = model.TimeZone;

                if (profilePicture != null)
                {
                    var fileName = $"{user.Id}_{DateTime.Parse("2025-03-11 12:43:07"):yyyyMMddHHmmss}{Path.GetExtension(profilePicture.FileName)}";
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(stream);
                    }

                    user.ProfilePicture = $"/uploads/profiles/{fileName}";
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
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
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManager.ChangePasswordAsync(user,
                    model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["Success"] = "Password changed successfully!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Select(u => new UserManagementViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault() ?? "User",
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    IsActive = u.IsActive,
                    ExpenseCount = u.Expenses.Count,
                    TotalExpenses = u.TotalExpenses
                })
                .ToListAsync();

            return View(users);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"User status updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            TempData["Success"] = $"User role updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}