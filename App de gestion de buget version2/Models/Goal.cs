using System;
using System.Collections.Generic;

namespace App_de_gestion_de_buget_version2.Models;

public partial class Goal
{
    public int GoalId { get; set; }

    public string Titre { get; set; } = null!;

    public decimal TargetAmount { get; set; }

    public decimal? CurrentSaved { get; set; }

    public DateTime Deadline { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
