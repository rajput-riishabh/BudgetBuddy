using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BudgetBuddy.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Parse("2025-03-11 15:43:11");
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string PreferredCurrency { get; set; } = "USD";
        public string TimeZone { get; set; } = "UTC";

        // Navigation Properties
        public virtual ICollection<Expense> Expenses { get; set; }
        public virtual ICollection<Budget> Budgets { get; set; }
        public virtual ICollection<Category> CreatedCategories { get; set; }

        public ApplicationUser()
        {
            Expenses = new List<Expense>();
            Budgets = new List<Budget>();
            CreatedCategories = new List<Category>();
        }

        // Helper Properties
        public string FullName => $"{FirstName} {LastName}";

        public decimal TotalExpenses => Expenses.Sum(e => e.Amount);

        public decimal CurrentMonthExpenses => Expenses
            .Where(e => e.Date.Year == DateTime.Parse("2025-03-11 12:43:07").Year &&
                       e.Date.Month == DateTime.Parse("2025-03-11 12:43:07").Month)
            .Sum(e => e.Amount);
    }
}