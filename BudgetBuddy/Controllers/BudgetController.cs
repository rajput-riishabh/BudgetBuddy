using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BudgetBuddy.Models;
using BudgetBuddy.Models.ViewModels;
using System.Security.Claims;
using BudgetBuddy.Services;
using System;
using System.Linq;
using BudgetBuddy.Models.Enums;

namespace BudgetBuddy.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly AppDbContext _context;

        public BudgetController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentDate = DateTime.Now;
            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Get current month's budgets with categories and their expenses
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId &&
                           b.StartDate <= currentDate &&
                           b.EndDate >= currentDate)
                .ToListAsync();

            // Get expenses for the current month
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.Date >= startOfMonth &&
                           e.Date <= endOfMonth)
                .ToListAsync();

            // Calculate budget status for each category
            var budgetStatuses = budgets.Select(b => new BudgetStatusViewModel
            {
                BudgetId = b.BudgetId,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name,
                BudgetAmount = b.Amount,
                SpentAmount = expenses.Where(e => e.CategoryId == b.CategoryId).Sum(e => e.Amount),
                StartDate = b.StartDate,
                EndDate = b.EndDate
            }).ToList();

            // Calculate overall budget metrics
            var viewModel = new BudgetIndexViewModel
            {
                BudgetStatuses = budgetStatuses,
                TotalBudget = budgetStatuses.Sum(b => b.BudgetAmount),
                TotalSpent = budgetStatuses.Sum(b => b.SpentAmount),
                CurrentMonth = currentDate.ToString("MMMM yyyy"),
                DaysRemainingInMonth = (endOfMonth - currentDate).Days + 1
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            var viewModel = new BudgetViewModel
            {
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.Date.AddMonths(1).AddDays(-1),
                Categories = categories
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BudgetViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                // Check if budget already exists for this category and date range
                var existingBudget = await _context.Budgets
                    .FirstOrDefaultAsync(b => b.UserId == userId &&
                                            b.CategoryId == viewModel.CategoryId &&
                                            ((b.StartDate <= viewModel.StartDate && b.EndDate >= viewModel.StartDate) ||
                                             (b.StartDate <= viewModel.EndDate && b.EndDate >= viewModel.EndDate)));

                if (existingBudget != null)
                {
                    ModelState.AddModelError("", "A budget already exists for this category during the specified period.");
                    viewModel.Categories = await _context.Categories.ToListAsync();
                    return View(viewModel);
                }

                var budget = new Budget
                {
                    UserId = userId,
                    CategoryId = viewModel.CategoryId,
                    Amount = viewModel.Amount,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate
                };

                _context.Budgets.Add(budget);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Budget created successfully!";
                return RedirectToAction(nameof(Index));
            }

            viewModel.Categories = await _context.Categories.ToListAsync();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);

            if (budget == null)
            {
                return NotFound();
            }

            var viewModel = new BudgetViewModel
            {
                BudgetId = budget.BudgetId,
                CategoryId = budget.CategoryId,
                Amount = budget.Amount,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                Categories = await _context.Categories.ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BudgetViewModel viewModel)
        {
            if (id != viewModel.BudgetId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    var budget = await _context.Budgets
                        .FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);

                    if (budget == null)
                    {
                        return NotFound();
                    }

                    // Check if budget already exists for this category and date range (excluding current budget)
                    var existingBudget = await _context.Budgets
                        .FirstOrDefaultAsync(b => b.BudgetId != id &&
                                                b.UserId == userId &&
                                                b.CategoryId == viewModel.CategoryId &&
                                                ((b.StartDate <= viewModel.StartDate && b.EndDate >= viewModel.StartDate) ||
                                                 (b.StartDate <= viewModel.EndDate && b.EndDate >= viewModel.EndDate)));

                    if (existingBudget != null)
                    {
                        ModelState.AddModelError("", "A budget already exists for this category during the specified period.");
                        viewModel.Categories = await _context.Categories.ToListAsync();
                        return View(viewModel);
                    }

                    budget.CategoryId = viewModel.CategoryId;
                    budget.Amount = viewModel.Amount;
                    budget.StartDate = viewModel.StartDate;
                    budget.EndDate = viewModel.EndDate;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Budget updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BudgetExists(viewModel.BudgetId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            viewModel.Categories = await _context.Categories.ToListAsync();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);

            if (budget == null)
            {
                return NotFound();
            }

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Budget deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool BudgetExists(int id)
        {
            return _context.Budgets.Any(b => b.BudgetId == id);
        }

        // API endpoint for getting budget progress data
        [HttpGet]
        public async Task<IActionResult> GetBudgetProgress()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentDate = DateTime.Now;
            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var budgetProgress = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId &&
                           b.StartDate <= currentDate &&
                           b.EndDate >= currentDate)
                .Select(b => new
                {
                    CategoryName = b.Category.Name,
                    BudgetAmount = b.Amount,
                    SpentAmount = _context.Expenses
                        .Where(e => e.UserId == userId &&
                                   e.CategoryId == b.CategoryId &&
                                   e.Date >= startOfMonth &&
                                   e.Date <= endOfMonth)
                        .Sum(e => e.Amount)
                })
                .ToListAsync();

            return Json(budgetProgress);
        }
    }
}