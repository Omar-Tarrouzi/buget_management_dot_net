using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App_de_gestion_de_buget_version2.Models
{
    public class Wallet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? WalletId { get; set; }
        public decimal? Balance { get; set; }

        // RELATION UTILISATEUR (string au lieu de int)
        public string UserId { get; set; } = null!;
        // User navigation removed for Mongo separation
        // public IdentityUser User { get; set; } = null!;

        public virtual ICollection<MonthlyPayment> MonthlyPayments { get; set; } = new List<MonthlyPayment>();
        public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}