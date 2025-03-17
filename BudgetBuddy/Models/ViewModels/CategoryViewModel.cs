using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetBuddy.Models.ViewModels 
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s&-]+$", ErrorMessage = "Category name can only contain letters, numbers, spaces, & and -")]
        public string Name { get; set; }

        public bool IsPredefined { get; set; }
        public int ExpenseCount { get; set; }
        public decimal TotalExpenses { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }

    }
}