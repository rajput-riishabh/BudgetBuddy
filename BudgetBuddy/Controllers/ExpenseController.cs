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
    public class ExpenseController : Controller
    {
        private readonly AppDbContext _context;

        public ExpenseController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortOrder = "", string currentFilter = "", string searchString = "", int? pageNumber = 1)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["AmountSortParm"] = sortOrder == "amount" ? "amount_desc" : "amount";
            ViewData["CategorySortParm"] = sortOrder == "category" ? "category_desc" : "category";
            ViewData["CurrentFilter"] = searchString ?? currentFilter;

            var expenses = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);

            // Search functionality
            if (!string.IsNullOrEmpty(searchString))
            {
                expenses = expenses.Where(e =>
                    e.Description.Contains(searchString) ||
                    e.Category.Name.Contains(searchString));
            }

            // Sorting
            expenses = sortOrder switch
            {
                "date_desc" => expenses.OrderByDescending(e => e.Date),
                "amount" => expenses.OrderBy(e => e.Amount),
                "amount_desc" => expenses.OrderByDescending(e => e.Amount),
                "category" => expenses.OrderBy(e => e.Category.Name),
                "category_desc" => expenses.OrderByDescending(e => e.Category.Name),
                _ => expenses.OrderByDescending(e => e.Date)
            };

            int pageSize = 10;
            var paginatedExpenses = await PaginatedList<Expense>.CreateAsync(expenses, pageNumber ?? 1, pageSize);

            // Get monthly total
            var currentMonth = DateTime.Now;
            var monthStart = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthlyTotal = await expenses
                .Where(e => e.Date >= monthStart && e.Date <= monthEnd)
                .SumAsync(e => e.Amount);

            ViewData["MonthlyTotal"] = monthlyTotal;

            return View(paginatedExpenses);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            var viewModel = new ExpenseViewModel
            {
                Date = DateTime.Now,
                Categories = categories
                    .Select(c => new CategoryViewModel
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        IsPredefined = c.IsPredefined // Include IsPredefined mapping
                    })
                    .ToList()
            };
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var expense = new Expense
                {
                    UserId = userId,
                    CategoryId = viewModel.CategoryId,
                    Amount = viewModel.Amount,
                    Description = viewModel.Description,
                    Date = viewModel.Date
                };

                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                // Check if budget is exceeded
                var budget = await _context.Budgets
                    .FirstOrDefaultAsync(b => b.UserId == userId &&
                                            b.CategoryId == viewModel.CategoryId &&
                                            b.StartDate <= viewModel.Date &&
                                            b.EndDate >= viewModel.Date);

                if (budget != null)
                {
                    var totalExpenses = await _context.Expenses
                        .Where(e => e.UserId == userId &&
                                  e.CategoryId == viewModel.CategoryId &&
                                  e.Date >= budget.StartDate &&
                                  e.Date <= budget.EndDate)
                        .SumAsync(e => e.Amount);

                    if (totalExpenses > budget.Amount)
                    {
                        TempData["Warning"] = $"Budget exceeded for {(await _context.Categories.FindAsync(viewModel.CategoryId))?.Name}!";
                    }
                }

                TempData["Success"] = "Expense added successfully!";
                return RedirectToAction(nameof(Index));
            }

            viewModel.Categories = await _context.Categories
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    IsPredefined = c.IsPredefined // Include IsPredefined mapping
                })
                .ToListAsync();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.ExpenseId == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            var viewModel = new ExpenseViewModel
            {
                ExpenseId = expense.ExpenseId,
                CategoryId = expense.CategoryId,
                Amount = expense.Amount,
                Description = expense.Description,
                Date = expense.Date,
                Categories = _context.Categories
                   .Select(c => new CategoryViewModel
                   {
                       CategoryId = c.CategoryId,
                       Name = c.Name,
                       IsPredefined = c.IsPredefined // Include IsPredefined mapping
                   })
                   .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseViewModel viewModel)
        {
            if (id != viewModel.ExpenseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    var expense = await _context.Expenses
                        .FirstOrDefaultAsync(e => e.ExpenseId == id && e.UserId == userId);

                    if (expense == null)
                    {
                        return NotFound();
                    }

                    expense.CategoryId = viewModel.CategoryId;
                    expense.Amount = viewModel.Amount;
                    expense.Description = viewModel.Description;
                    expense.Date = viewModel.Date;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Expense updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(viewModel.ExpenseId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            viewModel.Categories = await _context.Categories
                 .Select(c => new CategoryViewModel
                 {
                     CategoryId = c.CategoryId,
                     Name = c.Name,
                     IsPredefined = c.IsPredefined // Include IsPredefined mapping
                 })
                 .ToListAsync();
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.ExpenseId == id && e.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Expense deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.ExpenseId == id);
        }

        // API endpoint for getting monthly expenses for charts
        [HttpGet]
        public async Task<IActionResult> GetMonthlyExpenses()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var sixMonthsAgo = DateTime.Now.AddMonths(-5).Date;

            var monthlyExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= sixMonthsAgo)
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Total = g.Sum(e => e.Amount)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return Json(monthlyExpenses);
        }
    }
}