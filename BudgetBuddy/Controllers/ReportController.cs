using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BudgetBuddy.Models;
using BudgetBuddy.Models.ViewModels;
using System.Security.Claims;
using System.Text;
using System;
using System.Linq;
using BudgetBuddy.Models.Enums;

using BudgetBuddy.Services;

namespace BudgetBuddy.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly HashSet<int> _predefinedCategoryIds = new HashSet<int> { 1, 2, 3, 4 };

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentDate = DateTime.Parse("2025-03-11 12:24:49");

            // Default to current month if dates not specified
            startDate ??= new DateTime(currentDate.Year, currentDate.Month, 1);
            endDate ??= startDate.Value.AddMonths(1).AddDays(-1);

            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId &&
                           e.Date >= startDate &&
                           e.Date <= endDate)
                .ToListAsync();

            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId &&
                           b.StartDate <= endDate &&
                           b.EndDate >= startDate)
                .ToListAsync();

            var categories = await _context.Categories.ToListAsync();

            var viewModel = new ReportIndexViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalExpenses = expenses.Sum(e => e.Amount),
                TotalBudget = budgets.Sum(b => b.Amount),
                ExpenseCount = expenses.Count,
                CategoryCount = categories.Count,

                CategoryExpenses = categories
                    .Select(c => new CategoryReportViewModel
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.Name,
                        IsPredefined = _predefinedCategoryIds.Contains(c.CategoryId),
                        ExpenseCount = expenses.Count(e => e.CategoryId == c.CategoryId),
                        TotalAmount = expenses
                            .Where(e => e.CategoryId == c.CategoryId)
                            .Sum(e => e.Amount),
                        BudgetAmount = budgets
                            .FirstOrDefault(b => b.CategoryId == c.CategoryId)?.Amount ?? 0
                    })
                    .Where(c => c.ExpenseCount > 0)
                    .OrderByDescending(c => c.TotalAmount)
                    .ToList(),

                DailyExpenses = expenses
                    .GroupBy(e => e.Date.Date)
                    .Select(g => new DailyExpenseViewModel
                    {
                        Date = g.Key,
                        TotalAmount = g.Sum(e => e.Amount),
                        ExpenseCount = g.Count()
                    })
                    .OrderByDescending(d => d.Date)
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CategoryDetails(int id, DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentDate = DateTime.Parse("2025-03-11 12:24:49");

            startDate ??= new DateTime(currentDate.Year, currentDate.Month, 1);
            endDate ??= startDate.Value.AddMonths(1).AddDays(-1);

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId &&
                           e.CategoryId == id &&
                           e.Date >= startDate &&
                           e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId &&
                                        b.CategoryId == id &&
                                        b.StartDate <= endDate &&
                                        b.EndDate >= startDate);

            var viewModel = new CategoryReportViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.Name,
                IsPredefined = _predefinedCategoryIds.Contains(category.CategoryId),
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalExpenses = expenses.Sum(e => e.Amount),
                ExpenseCount = expenses.Count,
                BudgetAmount = budget?.Amount ?? 0,
                Expenses = expenses
                    .Select(e => new ExpenseViewModel
                    {
                        ExpenseId = e.ExpenseId,
                        Amount = e.Amount,
                        Description = e.Description,
                        Date = e.Date
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Export(DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentDate = DateTime.Parse("2025-03-11 12:24:49");

            startDate ??= new DateTime(currentDate.Year, currentDate.Month, 1);
            endDate ??= startDate.Value.AddMonths(1).AddDays(-1);

            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId &&
                           e.Date >= startDate &&
                           e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Date,Category,Description,Amount");

            foreach (var expense in expenses)
            {
                csvBuilder.AppendLine($"{expense.Date:yyyy-MM-dd},{expense.Category.Name}," +
                    $"\"{expense.Description.Replace("\"", "\"\"")}\",{expense.Amount}");
            }

            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            var fileName = $"expenses_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.csv";

            return File(bytes, "text/csv", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> MonthlyTrends(int months = 12)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentDate = DateTime.Parse("2025-03-11 12:24:49");
            var startDate = currentDate.AddMonths(-months + 1).Date;

            var trends = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.Date >= startDate)
                .GroupBy(e => new { e.Date.Year, e.Date.Month, e.CategoryId, e.Category.Name })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    Total = g.Sum(e => e.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var viewModel = new MonthlyTrendViewModel
            {
                Months = months,
                StartDate = startDate,
                EndDate = currentDate,
                Categories = await _context.Categories // Corrected: Map Category to CategoryViewModel
                                .Select(c => new CategoryViewModel // Projection to CategoryViewModel
                                {
                                    CategoryId = c.CategoryId,
                                    Name = c.Name,
                                    IsPredefined = _predefinedCategoryIds.Contains(c.CategoryId),
                                    // Add other mappings if needed, like ExpenseCount, TotalExpenses, etc., if you intend to use them in the MonthlyTrends view.
                                })
                                .ToListAsync(),
                MonthlyData = trends
                    .GroupBy(t => new { t.CategoryId, t.CategoryName })
                    .Select(g => new CategoryTrendViewModel
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.CategoryName,
                        MonthlyAmounts = g.ToDictionary( // Corrected: DateTime key for MonthlyAmounts
                            x => new DateTime(x.Year, x.Month, 1),
                            x => x.Total
                        )
                    })
                    .ToList()
            };

            return View(viewModel);
        }
    }
}