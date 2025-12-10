using Microsoft.AspNetCore.Identity;

namespace App_de_gestion_de_buget_version2.Models
{
    public class CategoryBudget
    {
        public int CategoryBudgetId { get; set; }
        
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
        
        public int Year { get; set; }
        public int Month { get; set; }
        
        public decimal MaxAmount { get; set; }
        
        // RELATION UTILISATEUR
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
    }
}


