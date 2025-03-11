﻿using System.ComponentModel.DataAnnotations;

namespace BudgetBuddy.Models
{
    public class PasswordReset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Token { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Parse("2025-03-11 14:28:48");

        // Navigation Property
        public virtual ApplicationUser User { get; set; }
    }
}
