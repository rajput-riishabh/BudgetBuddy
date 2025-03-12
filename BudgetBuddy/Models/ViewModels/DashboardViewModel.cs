using System;
using System.Collections.Generic;
using BudgetBuddy.Models.Enums;

namespace BudgetBuddy.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Summary Statistics
        public decimal TotalExpenses { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal RemainingBudget { get; set; }
        public int TotalCategories { get; set; }
        public int ActiveBudgets { get; set; }

        // Detailed Data
        public IEnumerable<CategoryExpenseSummary> CategoryExpenses { get; set; }
        public IEnumerable<MonthlyExpenseSummary> MonthlyExpenses { get; set; } // Corrected Name: Using MonthlyExpenses for the chart data
        public Dictionary<string, BudgetStatusInfo> BudgetStatuses { get; set; }
        public List<BudgetStatusInfo> BudgetStatusesInfo { get; set; }

        public DashboardViewModel()
        {
            CategoryExpenses = new List<CategoryExpenseSummary>();
            MonthlyExpenses = new List<MonthlyExpenseSummary>(); // Initialize MonthlyExpenses correctly
            BudgetStatuses = new Dictionary<string, BudgetStatusInfo>();
        }


        public class CategoryExpenseSummary
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string IconClass { get; set; }
            public decimal Amount { get; set; }
            public decimal BudgetAmount { get; set; }
            public decimal PercentageUsed { get; set; } // Changed to PercentageUsed to match HomeController and error message
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

        public class BudgetStatusSummary
        {
            public string CategoryName { get; set; } // Changed from 'Category' to 'CategoryName' to match HomeController and error
            public decimal BudgetAmount { get; set; } // Added BudgetAmount as per error message
            public decimal SpentAmount { get; set; } // Added SpentAmount as per error message
            public decimal RemainingAmount { get; set; } // Added RemainingAmount as per error message
            public bool IsExceeded { get; set; } // Added IsExceeded as per error message
            public decimal PercentageUsed { get; set; } // Kept PercentageUsed as it seems to be used in HomeController
            public BudgetStatus Status { get; set; } // Kept Status
        }

    }
}