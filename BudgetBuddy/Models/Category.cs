using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetBuddy.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z0-9\s&-]+$", ErrorMessage = "Category name can only contain letters, numbers, spaces, & and -")]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        // Auditing Properties
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [InverseProperty("Category")]
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        [InverseProperty("Category")]
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

        // Helper Properties
        [NotMapped]
        public bool IsPredefined => CategoryId >= 1 && CategoryId <= 4;

        [NotMapped]
        public decimal CurrentMonthExpenses => Expenses
            .Where(e => e.Date.Year == DateTime.Now.Year &&
                       e.Date.Month == DateTime.Now.Month)
            .Sum(e => e.Amount);

        [NotMapped]
        public decimal CurrentMonthBudget => Budgets
            .Where(b => b.StartDate <= DateTime.Now &&
                       b.EndDate >= DateTime.Now)
            .Sum(b => b.Amount);

        [NotMapped]
        public bool HasActiveBudget => Budgets.Any(b =>
            b.StartDate <= DateTime.Now &&
            b.EndDate >= DateTime.Now);

        // Methods
        public bool CanBeModifiedBy(int userId, bool isAdmin)
        {
            if (isAdmin) return true;
            if (IsPredefined) return false;
            return CreatedBy == userId;
        }

        public bool CanBeDeletedBy(int userId, bool isAdmin)
        {
            if (Expenses.Any()) return false;
            if (isAdmin) return true;
            if (IsPredefined) return false;
            return CreatedBy == userId;
        }

        public decimal GetExpensesForPeriod(DateTime startDate, DateTime endDate)
        {
            return Expenses
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .Sum(e => e.Amount);
        }

        public decimal GetBudgetForPeriod(DateTime startDate, DateTime endDate)
        {
            var budgets = Budgets
                .Where(b => b.StartDate <= endDate && b.EndDate >= startDate)
                .ToList();

            if (!budgets.Any()) return 0;

            // If multiple budgets exist for the period, calculate the proportional amount
            return budgets.Sum(b =>
            {
                var overlapStart = b.StartDate > startDate ? b.StartDate : startDate;
                var overlapEnd = b.EndDate < endDate ? b.EndDate : endDate;
                var totalDays = (b.EndDate - b.StartDate).Days + 1;
                var overlapDays = (overlapEnd - overlapStart).Days + 1;
                return b.Amount * overlapDays / totalDays;
            });
        }

        public BudgetStatus GetBudgetStatus()
        {
            if (!HasActiveBudget) return BudgetStatus.NoBudget;

            var currentExpenses = CurrentMonthExpenses;
            var currentBudget = CurrentMonthBudget;
            var percentage = currentBudget > 0 ? (currentExpenses / currentBudget) * 100 : 0;

            return percentage switch
            {
                >= 100 => BudgetStatus.Exceeded,
                >= 90 => BudgetStatus.Warning,
                >= 75 => BudgetStatus.Attention,
                > 0 => BudgetStatus.Good,
                _ => BudgetStatus.NoBudget
            };
        }
    }

    public enum BudgetStatus
    {
        NoBudget,
        Good,
        Attention,
        Warning,
        Exceeded
    }
}