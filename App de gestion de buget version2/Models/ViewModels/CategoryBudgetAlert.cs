namespace App_de_gestion_de_buget_version2.Models.ViewModels
{
    public class CategoryBudgetAlert
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal OverAmount { get; set; }
        public decimal OverPercentage { get; set; }
    }
}


