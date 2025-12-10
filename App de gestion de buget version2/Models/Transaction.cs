namespace App_de_gestion_de_buget_version2.Models
{
    public enum TransactionType
    {
        Expense = 0,
        Income = 1
    }

    public class Transaction
    {
        public int TransactionId { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
        public decimal Montant { get; set; }
        public int WalletId { get; set; }
        public int? CategoryId { get; set; }

        // Type de transaction : revenu ou dépense
        public TransactionType Type { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Wallet Wallet { get; set; } = null!;
    }
}