using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetBuddy.Models
{
    public class Budget
    {
        public int BudgetId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Category Category { get; set; }
    }
}