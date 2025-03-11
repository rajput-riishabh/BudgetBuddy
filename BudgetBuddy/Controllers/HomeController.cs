using Microsoft.AspNetCore.Mvc;
using BudgetBuddy.Models;
using BudgetBuddy.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Diagnostics;
using System;
using System.Linq;
using BudgetBuddy.Models.Enums;
using BudgetBuddy.Services;

namespace BudgetBuddy.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Dashboard));
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentDate = DateTime.UtcNow;
            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Get current month's expenses
            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.Date >= startOfMonth && e.Date <= endOfMonth)
                .ToListAsync();

            // Get current month's budgets
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && b.StartDate <= currentDate && b.EndDate >= currentDate)
                .ToListAsync();

            // Get last 6 months expenses for trend
            var sixMonthsAgo = startOfMonth.AddMonths(-5);
            var monthlyExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= sixMonthsAgo)
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new DashboardViewModel.MonthlyExpenseSummary
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Amount = g.Sum(e => e.Amount)
                })
                .OrderBy(m => m.Month)
                .ToListAsync();

            var categoryExpenses = expenses
                .GroupBy(e => e.Category)
                .Select(g => new DashboardViewModel.CategoryExpenseSummary
                {
                    CategoryName = g.Key.Name,
                    Amount = g.Sum(e => e.Amount),
                    BudgetAmount = budgets.FirstOrDefault(b => b.CategoryId == g.Key.CategoryId)?.Amount ?? 0,
                    PercentageUsed = ((g.Sum(e => e.Amount) /
                        (budgets.FirstOrDefault(b => b.CategoryId == g.Key.CategoryId)?.Amount ?? 1)) * 100)
                })
                .ToList();

            var budgetStatuses = budgets
                .Select(b => new DashboardViewModel.BudgetStatus
                {
                    CategoryName = b.Category.Name,
                    BudgetAmount = b.Amount,
                    SpentAmount = expenses
                        .Where(e => e.CategoryId == b.CategoryId)
                        .Sum(e => e.Amount),
                    RemainingAmount = b.Amount - expenses
                        .Where(e => e.CategoryId == b.CategoryId)
                        .Sum(e => e.Amount),
                    IsExceeded = expenses
                        .Where(e => e.CategoryId == b.CategoryId)
                        .Sum(e => e.Amount) > b.Amount
                })
                .ToList();

            var dashboardViewModel = new DashboardViewModel
            {
                TotalExpenses = expenses.Sum(e => e.Amount),
                TotalBudget = budgets.Sum(b => b.Amount),
                RemainingBudget = budgets.Sum(b => b.Amount) - expenses.Sum(e => e.Amount),
                CategoryExpenses = categoryExpenses,
                MonthlyExpenses = monthlyExpenses,
                BudgetStatuses = budgetStatuses
            };

            return View(dashboardViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}