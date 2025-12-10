using App_de_gestion_de_buget_version2.Models;

namespace App_de_gestion_de_buget_version2.Models.ViewModels
{
    public class TransactionIndexViewModel
    {
        public IEnumerable<Transaction> Transactions { get; set; } = new List<Transaction>();
        public List<CategoryBreakdownItem> CategoryBreakdown { get; set; } = new List<CategoryBreakdownItem>();
        public decimal TotalExpenses { get; set; }
        public decimal TotalIncomes { get; set; }
    }
}


