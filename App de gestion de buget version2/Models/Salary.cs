using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App_de_gestion_de_buget_version2.Models
{
    public class Salary
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? SalaryId { get; set; }
        public decimal Montant { get; set; }
        public DateTime Payday { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string WalletId { get; set; } = null!;

        public virtual Wallet Wallet { get; set; } = null!;
    }
}