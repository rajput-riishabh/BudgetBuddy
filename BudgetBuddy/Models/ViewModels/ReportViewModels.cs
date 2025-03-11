using System;
using System.Collections.Generic;
using BudgetBuddy.Models.Enums;

namespace BudgetBuddy.Models.ViewModels
{

    public class ReportViewModels
    {
        public ExpenseReportViewModel ExpenseReport { get; set; }
        public BudgetReportViewModel BudgetReport { get; set; }

        public ReportViewModels()
        {
            ExpenseReport = new ExpenseReportViewModel();
            BudgetReport = new BudgetReportViewModel();
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
            StartDate = DateTime.Parse("2025-03-11 16:46:27").AddMonths(-1);
            EndDate = DateTime.Parse("2025-03-11 16:46:27");
            Items = new List<ExpenseReportItem>();
        }
    }

    public class ExpenseReportItem
    {
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public int Count { get; set; }
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
    }
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
    }

    public class CategoryReportViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsPredefined { get; set; }
        public int ExpenseCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal BudgetAmount { get; set; }

        public decimal RemainingBudget => BudgetAmount - TotalAmount;
        public double BudgetUsagePercentage => BudgetAmount == 0 ? 0 : (double)((TotalAmount / BudgetAmount) * 100);
    }

    public class DailyExpenseViewModel
    {
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public int ExpenseCount { get; set; }
    }

    public class CategoryReportDetailViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsPredefined { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalExpenses { get; set; }
        public int ExpenseCount { get; set; }
        public decimal BudgetAmount { get; set; }
        public List<ExpenseViewModel> Expenses { get; set; }

        public decimal RemainingBudget => BudgetAmount - TotalExpenses;
        public double BudgetUsagePercentage => BudgetAmount == 0 ? 0 : (double)((TotalExpenses / BudgetAmount) * 100);
    }

    public class MonthlyTrendViewModel
    {
        public int Months { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Category> Categories { get; set; }
        public List<CategoryTrendViewModel> MonthlyData { get; set; }
    }

    public class CategoryTrendViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Dictionary<DateTime, decimal> MonthlyAmounts { get; set; }
    }
}