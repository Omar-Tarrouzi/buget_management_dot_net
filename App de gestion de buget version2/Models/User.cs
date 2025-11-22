using System;
using System.Collections.Generic;

namespace App_de_gestion_de_buget_version2.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();

    public virtual Wallet? Wallet { get; set; }
}
