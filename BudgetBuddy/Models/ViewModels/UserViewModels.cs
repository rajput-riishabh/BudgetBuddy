using System.ComponentModel.DataAnnotations;

namespace BudgetBuddy.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string ProfilePicture { get; set; }

        [Display(Name = "Preferred Currency")]
        public string PreferredCurrency { get; set; }

        public string TimeZone { get; set; }

        // Statistics
        public int TotalExpenses { get; set; }
        public int TotalBudgets { get; set; }
        public int CustomCategories { get; set; }
        public decimal CurrentMonthSpending { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UserManagementViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public int ExpenseCount { get; set; }
        public decimal TotalExpenses { get; set; }
    }
}