namespace BudgetBuddy.Models.ViewModels
{
    public class BudgetIndexViewModel
    {
        public IEnumerable<BudgetStatusViewModel> BudgetStatuses { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public string CurrentMonth { get; set; }
        public int DaysRemainingInMonth { get; set; }

        public decimal RemainingBudget => TotalBudget - TotalSpent;
        public double OverallPercentageUsed => TotalBudget == 0 ? 0 : (double)((TotalSpent / TotalBudget) * 100);
    }
}