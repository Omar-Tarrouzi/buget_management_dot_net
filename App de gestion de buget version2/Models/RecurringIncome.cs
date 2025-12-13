using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App_de_gestion_de_buget_version2.Models
{
    public class RecurringIncome
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? RecurringIncomeId { get; set; }
        public decimal Montant { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? LastProcessedDate { get; set; }
        public string Description { get; set; } = string.Empty;
        
        [BsonRepresentation(BsonType.ObjectId)]
        public string WalletId { get; set; } = null!;
        public virtual Wallet Wallet { get; set; } = null!;
        
        // RELATION UTILISATEUR
        public string UserId { get; set; } = null!;
        // User navigation removed for Mongo separation
        // public IdentityUser User { get; set; } = null!;
    }
}
