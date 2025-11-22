using System;
using System.Collections.Generic;

namespace App_de_gestion_de_buget_version2.Models;

public partial class Salary
{
    public int SalaryId { get; set; }

    public decimal Montant { get; set; }

    public DateTime Payday { get; set; }

    public int WalletId { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
