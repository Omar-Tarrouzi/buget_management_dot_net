using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App_de_gestion_de_buget_version2.Models
{
    public class Budget
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? BudgetId { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public decimal PlannedAmount { get; set; }

        // RELATION UTILISATEUR
        public string UserId { get; set; } = null!;
        // User navigation removed for Mongo separation
        // public IdentityUser User { get; set; } = null!;
    }
}
