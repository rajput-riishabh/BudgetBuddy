namespace BudgetBuddy.Models.ViewModels
{
    public class BudgetStatusViewModel
    {
        public int BudgetId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal RemainingAmount => BudgetAmount - SpentAmount;
        public double PercentageUsed => BudgetAmount == 0 ? 0 : (double)((SpentAmount / BudgetAmount) * 100);
        public bool IsExceeded => SpentAmount > BudgetAmount;
    }
}