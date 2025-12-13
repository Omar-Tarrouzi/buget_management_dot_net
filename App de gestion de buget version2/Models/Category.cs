using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App_de_gestion_de_buget_version2.Models
{
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? CategoryId { get; set; }
        public string Nom { get; set; } = null!;

        // RELATION UTILISATEUR
        public string UserId { get; set; } = null!;
        // User navigation removed for Mongo separation
        // public IdentityUser User { get; set; } = null!;

        [BsonIgnore]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}