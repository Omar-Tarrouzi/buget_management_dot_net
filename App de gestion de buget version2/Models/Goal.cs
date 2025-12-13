using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App_de_gestion_de_buget_version2.Models
{
    public class Goal
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? GoalId { get; set; }
        public string Titre { get; set; } = null!;
        public decimal TargetAmount { get; set; }
        public decimal? CurrentSaved { get; set; }
        public DateTime Deadline { get; set; }

        // RELATION UTILISATEUR (string au lieu de int)
        public string UserId { get; set; } = null!;
        // User navigation removed for Mongo separation
        // public IdentityUser User { get; set; } = null!;
    }
}