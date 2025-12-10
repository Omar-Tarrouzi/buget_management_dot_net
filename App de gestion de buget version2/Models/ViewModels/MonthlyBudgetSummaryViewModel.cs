namespace App_de_gestion_de_buget_version2.Models.ViewModels
{
    public class MonthlyBudgetSummaryViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public decimal PlannedAmount { get; set; }
        public decimal Incomes { get; set; }
        public decimal Expenses { get; set; }

        public decimal Remaining => PlannedAmount - Expenses;
        
        public List<CategoryBreakdownItem> CategoryBreakdown { get; set; } = new List<CategoryBreakdownItem>();
    }
}


