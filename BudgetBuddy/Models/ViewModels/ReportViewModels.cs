using System;
using System.Collections.Generic;
using System.Globalization;
using BudgetBuddy.Models.Enums;

namespace BudgetBuddy.Models.ViewModels
{
    public class ReportIndexViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalBudget { get; set; }
        public int ExpenseCount { get; set; }
        public int CategoryCount { get; set; }
        public List<CategoryReportViewModel> CategoryExpenses { get; set; }
        public List<DailyExpenseViewModel> DailyExpenses { get; set; }

        public decimal RemainingBudget => TotalBudget - TotalExpenses;
        public double BudgetUsagePercentage => TotalBudget == 0 ? 0 : (double)((TotalExpenses / TotalBudget) * 100);

        public ReportIndexViewModel()
        {
            CategoryExpenses = new List<CategoryReportViewModel>();
            DailyExpenses = new List<DailyExpenseViewModel>();
            EndDate = DateTime.UtcNow;
            StartDate = EndDate.AddMonths(-1);
        }
    }

    public class ExpenseReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ExpenseReportItem> Items { get; set; }
        public decimal TotalAmount { get; set; }

        public ExpenseReportViewModel()
        {
            Items = new List<ExpenseReportItem>();
            EndDate = DateTime.UtcNow;
            StartDate = EndDate.AddMonths(-1);
        }
    }

    public class ExpenseReportItem
    {
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public int Count { get; set; }

        public ExpenseReportItem()
        {
            CategoryName = string.Empty;
        }
    }

    public class BudgetReportViewModel
    {
        public List<BudgetReportItem> Items { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal RemainingBudget { get; set; }

        public BudgetReportViewModel()
        {
            Items = new List<BudgetReportItem>();
        }
    }

    public class BudgetReportItem
    {
        public string CategoryName { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount => BudgetAmount - SpentAmount;
        public decimal Percentage => BudgetAmount == 0 ? 0 : (SpentAmount / BudgetAmount) * 100;
        public BudgetStatus Status { get; set; }

        public BudgetReportItem()
        {
            CategoryName = string.Empty;
        }
    }

    public class CategoryReportViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsPredefined { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalExpenses { get; set; }

        public int ExpenseCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal BudgetAmount { get; set; }

        public ICollection<ExpenseViewModel> Expenses { get; set; }

        public decimal RemainingBudget => BudgetAmount - TotalAmount;
        public decimal BudgetUsagePercentage => BudgetAmount == 0 ? 0 : (decimal)((TotalAmount / BudgetAmount) * 100);

        public CategoryReportViewModel()
        {
            CategoryName = string.Empty;
            StartDate = DateTime.ParseExact("2025-03-12 04:14:12", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).AddMonths(-1);
            EndDate = DateTime.ParseExact("2025-03-12 04:14:12", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            Expenses = new List<ExpenseViewModel>();
        }
    }

    public class MonthlyTrendViewModel
    {
        public int Months { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CategoryViewModel> Categories { get; set; }
        public List<CategoryTrendViewModel> MonthlyData { get; set; }

        public MonthlyTrendViewModel()
        {
            Categories = new List<CategoryViewModel>();
            MonthlyData = new List<CategoryTrendViewModel>();
            Months = 3; // Default to 3 months
            StartDate = EndDate.AddMonths(-3);
            EndDate = DateTime.UtcNow;
        }
    }

    public class CategoryTrendViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Dictionary<string, decimal> MonthlyAmounts { get; set; }

        public CategoryTrendViewModel()
        {
            CategoryName = string.Empty;
            MonthlyAmounts = new Dictionary<string, decimal>();
        }
    }

    public class DailyExpenseViewModel
    {
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public int ExpenseCount { get; set; }
    }
}