using System.Collections.Generic;

namespace App_de_gestion_de_buget_version2.Models.ViewModels
{
    public class SetBudgetViewModel
    {
        public Budget Budget { get; set; } = new Budget();
        public List<CategoryBudgetInputModel> CategoryBudgets { get; set; } = new List<CategoryBudgetInputModel>();
    }

    public class CategoryBudgetInputModel
    {
        public string CategoryId { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public decimal? Limit { get; set; } // Optional limit
    }
}
