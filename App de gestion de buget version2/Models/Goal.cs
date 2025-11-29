using Microsoft.AspNetCore.Identity;

namespace App_de_gestion_de_buget_version2.Models
{
    public class Goal
    {
        public int GoalId { get; set; }
        public string Titre { get; set; } = null!;
        public decimal TargetAmount { get; set; }
        public decimal? CurrentSaved { get; set; }
        public DateTime Deadline { get; set; }

        // RELATION UTILISATEUR (string au lieu de int)
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
    }
}