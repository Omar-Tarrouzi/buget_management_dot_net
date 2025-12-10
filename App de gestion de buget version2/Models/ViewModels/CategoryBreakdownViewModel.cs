namespace App_de_gestion_de_buget_version2.Models.ViewModels
{
    public class CategoryBreakdownItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class CategoryBreakdownViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<CategoryBreakdownItem> Items { get; set; } = new List<CategoryBreakdownItem>();
    }
}


