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

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);
            if (wallet == null)
            {
                return View(new MonthlyBudgetSummaryViewModel
                {
                    Year = y,
                    Month = m,
                    PlannedAmount = 0m,
                    Incomes = 0m,
                    Expenses = 0m
                });
            }

            var salariesList = _context.Salaries
                .Where(s => s.WalletId == wallet.WalletId &&
                            s.Payday.Year == y &&
                            s.Payday.Month == m)
                .ToList();
            var salaries = salariesList.Sum(s => s.Montant);

            var variableIncomesList = _context.Transactions
                .Where(t => t.WalletId == wallet.WalletId &&
                            t.Type == TransactionType.Income &&
                            t.Date.HasValue &&
                            t.Date.Value.Year == y &&
                            t.Date.Value.Month == m)
                .ToList();
            var variableIncomes = variableIncomesList.Sum(t => t.Montant);

            var expensesList = _context.Transactions
                .Where(t => t.WalletId == wallet.WalletId &&
                            t.Type == TransactionType.Expense &&
                            t.Date.HasValue &&
                            t.Date.Value.Year == y &&
                            t.Date.Value.Month == m)
                .ToList();
            var expenses = expensesList.Sum(t => t.Montant);

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

            var wallet2 = _context.Wallets.FirstOrDefault(w => w.UserId == userId);
            if (wallet2 == null)
            {
                return View(new CategoryBreakdownViewModel
                {
                    Year = y,
                    Month = m,
                    Items = new List<CategoryBreakdownItem>()
                });
            }

            var transactions = _context.Transactions
                .Where(t => t.WalletId == wallet2.WalletId &&
                            t.Type == TransactionType.Expense &&
                            t.Date.HasValue &&
                            t.Date.Value.Year == y &&
                            t.Date.Value.Month == m)
                .ToList();

            // Manually load categories
            var categoryIds = transactions.Where(t => t.CategoryId != null).Select(t => t.CategoryId).Distinct().ToList();
            var categories = _context.Categories.Where(c => categoryIds.Contains(c.CategoryId)).ToList();
            
            foreach (var transaction in transactions)
            {
                if (transaction.CategoryId != null)
                {
                    transaction.Category = categories.FirstOrDefault(c => c.CategoryId == transaction.CategoryId);
                }
            }

            var items = transactions
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


