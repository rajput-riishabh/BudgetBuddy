using System.ComponentModel.DataAnnotations;

namespace BudgetBuddy.Models.ViewModels
{
    public class BudgetViewModel
    {
        public int BudgetId { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Budget Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        // Navigation property for categories dropdown
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    }
}