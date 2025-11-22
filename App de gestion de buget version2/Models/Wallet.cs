using System;
using System.Collections.Generic;

namespace App_de_gestion_de_buget_version2.Models;

public partial class Wallet
{
    public int WalletId { get; set; }

    public decimal? Balance { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<MonthlyPayment> MonthlyPayments { get; set; } = new List<MonthlyPayment>();

    public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
