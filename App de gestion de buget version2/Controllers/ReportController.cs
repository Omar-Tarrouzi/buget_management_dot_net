using App_de_gestion_de_buget_version2.Data;
using App_de_gestion_de_buget_version2.Models;
using App_de_gestion_de_buget_version2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace App_de_gestion_de_buget_version2.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportController> _logger;

        public ReportController(ApplicationDbContext context, ILogger<ReportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User not authenticated");
            return userId;
        }

        // GET: /Report/MonthlySummary
        public IActionResult MonthlySummary(int? year, int? month)
        {
            var userId = GetUserId();
            var now = DateTime.Now;

            int y = year ?? now.Year;
            int m = month ?? now.Month;

            var budget = _context.Budgets
                .FirstOrDefault(b => b.UserId == userId && b.Year == y && b.Month == m);

            var salaries = _context.Salaries
                .Include(s => s.Wallet)
                .Where(s => s.Wallet.UserId == userId &&
                            s.Payday.Year == y &&
                            s.Payday.Month == m)
                .Sum(s => (decimal?)s.Montant) ?? 0m;

            var variableIncomes = _context.Transactions
                .Include(t => t.Wallet)
                .Where(t => t.Wallet.UserId == userId &&
                            t.Type == TransactionType.Income &&
                            t.Date.HasValue &&
                            t.Date.Value.Year == y &&
                            t.Date.Value.Month == m)
                .Sum(t => (decimal?)t.Montant) ?? 0m;

            var expenses = _context.Transactions
                .Include(t => t.Wallet)
                .Where(t => t.Wallet.UserId == userId &&
                            t.Type == TransactionType.Expense &&
                            t.Date.HasValue &&
                            t.Date.Value.Year == y &&
                            t.Date.Value.Month == m)
                .Sum(t => (decimal?)t.Montant) ?? 0m;

            var vm = new MonthlyBudgetSummaryViewModel
            {
                Year = y,
                Month = m,
                PlannedAmount = budget?.PlannedAmount ?? 0m,
                Incomes = salaries + variableIncomes,
                Expenses = expenses
            };

            return View(vm);
        }

        // GET: /Report/CategoryBreakdown
        public IActionResult CategoryBreakdown(int? year, int? month)
        {
            var userId = GetUserId();
            var now = DateTime.Now;

            int y = year ?? now.Year;
            int m = month ?? now.Month;

            var items = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Wallet)
                .Where(t => t.Wallet.UserId == userId &&
                            t.Type == TransactionType.Expense &&
                            t.Date.HasValue &&
                            t.Date.Value.Year == y &&
                            t.Date.Value.Month == m)
                .GroupBy(t => t.Category != null ? t.Category.Nom : "Sans catégorie")
                .Select(g => new CategoryBreakdownItem
                {
                    CategoryName = g.Key,
                    Total = g.Sum(t => t.Montant)
                })
                .ToList();

            var vm = new CategoryBreakdownViewModel
            {
                Year = y,
                Month = m,
                Items = items
            };

            return View(vm);
        }

        // GET: /Report/IncomesVsExpenses
        public IActionResult IncomesVsExpenses(int? year, int? month)
        {
            // Pour l’instant, simple réutilisation du modèle de résumé mensuel
            return MonthlySummary(year, month);
        }
    }
}


