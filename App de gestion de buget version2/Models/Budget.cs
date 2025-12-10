using Microsoft.AspNetCore.Identity;

namespace App_de_gestion_de_buget_version2.Models
{
    public class Budget
    {
        public int BudgetId { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public decimal PlannedAmount { get; set; }

        // RELATION UTILISATEUR
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
    }
}


