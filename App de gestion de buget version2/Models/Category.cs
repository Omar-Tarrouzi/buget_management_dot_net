using Microsoft.AspNetCore.Identity;

namespace App_de_gestion_de_buget_version2.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Nom { get; set; } = null!;

        // RELATION UTILISATEUR
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}