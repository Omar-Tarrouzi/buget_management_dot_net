using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using App_de_gestion_de_buget_version2.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.EntityFrameworkCore.Extensions;

namespace App_de_gestion_de_buget_version2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<CategoryBudget> CategoryBudgets { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<MonthlyPayment> MonthlyPayments { get; set; }
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<RecurringIncome> RecurringIncomes { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure mappings
            modelBuilder.Entity<Wallet>().ToCollection("Wallets");
            modelBuilder.Entity<Transaction>().ToCollection("Transactions");
            modelBuilder.Entity<Category>().ToCollection("Categories");
            modelBuilder.Entity<Budget>().ToCollection("Budgets");
            modelBuilder.Entity<CategoryBudget>().ToCollection("CategoryBudgets");
            modelBuilder.Entity<Goal>().ToCollection("Goals");
            modelBuilder.Entity<MonthlyPayment>().ToCollection("MonthlyPayments");
            modelBuilder.Entity<Salary>().ToCollection("Salaries");
            modelBuilder.Entity<RecurringIncome>().ToCollection("RecurringIncomes");
            
            // Identity collections
            modelBuilder.Entity<IdentityUser>().ToCollection("Users");
            modelBuilder.Entity<IdentityRole>().ToCollection("Roles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToCollection("UserClaims");
            modelBuilder.Entity<IdentityUserRole<string>>().ToCollection("UserRoles");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToCollection("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToCollection("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToCollection("UserTokens");
        }
    }
}
