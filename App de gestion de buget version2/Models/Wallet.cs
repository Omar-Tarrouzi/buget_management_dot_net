using Microsoft.AspNetCore.Identity;

namespace App_de_gestion_de_buget_version2.Models
{
    public class Wallet
    {
        public int WalletId { get; set; }
        public decimal? Balance { get; set; }

        // RELATION UTILISATEUR (string au lieu de int)
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;

        public virtual ICollection<MonthlyPayment> MonthlyPayments { get; set; } = new List<MonthlyPayment>();
        public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}