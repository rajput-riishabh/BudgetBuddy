using BudgetBuddy.Models.Enums;

namespace BudgetBuddy.Models
{
    public class BudgetStatusInfo
    {
        public string CategoryName { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount => BudgetAmount - SpentAmount;
        public bool IsExceeded => SpentAmount > BudgetAmount;
        public BudgetStatus Status { get; set; }

        public BudgetStatusInfo()
        {
            CategoryName = string.Empty;
            BudgetAmount = 0;
            SpentAmount = 0;
            Status = BudgetStatus.NoBudget;
        }
    }
}