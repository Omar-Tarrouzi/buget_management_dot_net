namespace App_de_gestion_de_buget_version2.Models
{
    public class MonthlyPayment
    {
        // Renamed to follow EF Core conventions so the entity has a primary key
        public int MonthlyPaymentId { get; set; }
        public string Nom { get; set; } = null!;
        public decimal Montant { get; set; }
        public DateTime DueDate { get; set; }
        public int WalletId { get; set; }

        public virtual Wallet Wallet { get; set; } = null!;
    }
}