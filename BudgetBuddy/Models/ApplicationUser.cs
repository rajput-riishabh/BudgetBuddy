using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetBuddy.Models
{
    public class ApplicationUser
    {
        [Key]
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

        // Profile specific fields
        public string ProfilePicture { get; set; }

        // Preferences
        [Required]
        public string PreferredCurrency { get; set; } = "INR";

        [Required]
        public string TimeZone { get; set; } = "Asia/Kolkata";

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Expense> Expenses { get; set; }
        public virtual ICollection<Budget> Budgets { get; set; }
        public virtual ICollection<Category> CreatedCategories { get; set; }

        // Computed Properties
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public decimal TotalExpenses => Expenses?.Sum(e => e.Amount) ?? 0;

        [NotMapped]
        public decimal CurrentMonthExpenses => Expenses?
            .Where(e => e.Date.Year == DateTime.Now.Year &&
                       e.Date.Month == DateTime.Now.Month)
            .Sum(e => e.Amount) ?? 0;

        public ApplicationUser()
        {
            Expenses = new List<Expense>();
            Budgets = new List<Budget>();
            CreatedCategories = new List<Category>();
            CreatedAt = DateTime.Now;
            IsActive = true;
            PreferredCurrency = "INR";
            TimeZone = "Asia/Kolkata";
        }
    }
}