using System;
using System.Collections.Generic;

namespace App_de_gestion_de_buget_version2.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Nom { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
