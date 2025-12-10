namespace App_de_gestion_de_buget_version2.Models.ViewModels
{
    public class WalletDashboardViewModel
    {
        public decimal Balance { get; set; }
        public decimal CurrentMonthExpenses { get; set; }
        public decimal CurrentMonthIncomes { get; set; }
        public decimal CurrentMonthBudget { get; set; }
        public decimal BudgetUsagePercent { get; set; }
        public string BudgetHealthStatus { get; set; } = string.Empty; // "good", "warning", "danger"
        public decimal SavingsRate { get; set; }
        public int MonthsWithBudget { get; set; }
        public int MonthsOverBudget { get; set; }
        public List<CategoryBreakdownItem> CategoryBreakdown { get; set; } = new List<CategoryBreakdownItem>();
        public List<CategoryBudgetAlert> CategoryBudgetAlerts { get; set; } = new List<CategoryBudgetAlert>();
    }
}

