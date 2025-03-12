using System;
using System.Collections.Generic;
using BudgetBuddy.Models.Enums;

namespace BudgetBuddy.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Summary Statistics
        public decimal TotalExpenses { get; set; }
        public decimal MonthlyExpenses { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal RemainingBudget { get; set; }
        public int TotalCategories { get; set; }
        public int ActiveBudgets { get; set; }

        // Detailed Data
        public IEnumerable<CategoryExpenseSummary> CategoryExpenses { get; set; }
        public IEnumerable<MonthlyExpenseSummary> MonthlyTrend { get; set; }
        public Dictionary<string, BudgetStatusInfo> BudgetStatuses { get; set; }

        public List<BudgetStatusInfo> BudgetStatuses { get; set; }


        // Budget Overview
        //public decimal MonthlyBudgetLimit { get; set; }
        //public decimal MonthlyBudgetUsed { get; set; }
        //public BudgetStatus OverallBudgetStatus { get; set; }
        //public int DaysRemainingInMonth { get; set; }
        //public decimal DailyBudgetRequired { get; set; }

        public DashboardViewModel()
        {
            CategoryExpenses = new List<CategoryExpenseSummary>();
            MonthlyTrend = new List<MonthlyExpenseSummary>();
            BudgetStatuses = new Dictionary<string, BudgetStatusInfo>();
        }
    }

    public class CategoryExpenseSummary
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string IconClass { get; set; }
        public decimal Amount { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal Percentage { get; set; }
        public BudgetStatus Status { get; set; }
        public string ColorClass { get; set; }
    }

    public class MonthlyExpenseSummary
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal Percentage { get; set; }
        public BudgetStatus Status { get; set; }
    }

    //public class RecentTransaction
    //{
    //    public int ExpenseId { get; set; }
    //    public string Description { get; set; }
    //    public decimal Amount { get; set; }
    //    public string CategoryName { get; set; }
    //    public DateTime Date { get; set; }
    //    public string IconClass { get; set; }
    //}

    public class BudgetStatusSummary
    {
        public string Category { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal BudgetLimit { get; set; }
        public decimal PercentageUsed { get; set; }
        public BudgetStatus Status { get; set; }
    }
}