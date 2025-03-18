using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

namespace BudgetBuddy.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Profile specific fields (from ApplicationUser)
        public string ProfilePicture { get; set; }
        [Required]
        public string PreferredCurrency { get; set; } = "INR";
        [Required]
        public string TimeZone { get; set; } = "Asia/Kolkata";

        // Navigation properties
        public virtual ICollection<Expense> Expenses { get; set; }
        public virtual ICollection<Budget> Budgets { get; set; }
        public virtual ICollection<Category> CreatedCategories { get; set; } // Add this

        // Computed properties
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        [NotMapped]
        public decimal TotalExpenses => Expenses?.Sum(e => e.Amount) ?? 0;

        [NotMapped]
        public decimal CurrentMonthExpenses => Expenses?
            .Where(e => e.Date.Year == DateTime.Now.Year &&
                        e.Date.Month == DateTime.Now.Month)
            .Sum(e => e.Amount) ?? 0;

        public User()
        {
            Expenses = new List<Expense>();
            Budgets = new List<Budget>();
            CreatedCategories = new List<Category>(); // Initialize
            CreatedAt = DateTime.Now;
            IsActive = true;
            PreferredCurrency = "INR";
            TimeZone = "Asia/Kolkata";
        }
    }
}