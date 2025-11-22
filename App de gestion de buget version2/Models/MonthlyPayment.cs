using System;
using System.Collections.Generic;

namespace App_de_gestion_de_buget_version2.Models;

public partial class MonthlyPayment
{
    public int PaymentMid { get; set; }

    public string Nom { get; set; } = null!;

    public decimal Montant { get; set; }

    public DateTime DueDate { get; set; }

    public int WalletId { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
