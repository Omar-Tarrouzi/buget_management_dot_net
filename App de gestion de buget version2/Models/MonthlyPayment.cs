using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App_de_gestion_de_buget_version2.Models
{
    public class MonthlyPayment
    {
        // Renamed to follow EF Core conventions so the entity has a primary key
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? MonthlyPaymentId { get; set; }
        public string Nom { get; set; } = null!;
        public decimal Montant { get; set; }
        public DateTime DueDate { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string WalletId { get; set; } = null!;

        public virtual Wallet Wallet { get; set; } = null!;
    }
}