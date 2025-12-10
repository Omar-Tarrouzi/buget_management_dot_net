using Microsoft.AspNetCore.Identity;

namespace App_de_gestion_de_buget_version2.Models
{
    public class RecurringIncome
    {
        public int RecurringIncomeId { get; set; }
        public decimal Montant { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LastProcessedDate { get; set; }
        public string Description { get; set; } = string.Empty;
        
        public int WalletId { get; set; }
        public virtual Wallet Wallet { get; set; } = null!;
        
        // RELATION UTILISATEUR
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
    }
}


