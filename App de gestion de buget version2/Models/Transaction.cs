using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App_de_gestion_de_buget_version2.Models
{
    public enum TransactionType
    {
        Expense = 0,
        Income = 1
    }

    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? TransactionId { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
        public decimal Montant { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string WalletId { get; set; } = null!;
        [BsonRepresentation(BsonType.ObjectId)]
        public string? CategoryId { get; set; }

        // Type de transaction : revenu ou dépense
        public TransactionType Type { get; set; }

        [BsonIgnore]
        public virtual Category? Category { get; set; }
        [BsonIgnore]
        public virtual Wallet Wallet { get; set; } = null!;
    }
}