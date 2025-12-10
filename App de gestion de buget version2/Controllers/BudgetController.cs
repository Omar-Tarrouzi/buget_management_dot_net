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
    public class BudgetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BudgetController> _logger;

        public BudgetController(ApplicationDbContext context, ILogger<BudgetController> logger)
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

        // GET: /Budget
        public IActionResult Index()
        {
            var userId = GetUserId();
            var budgets = _context.Budgets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ToList();

            return View(budgets);
        }

        // GET: /Budget/SetBudget/{id?}
        public IActionResult SetBudget(int? id, int? year, int? month)
        {
            var userId = GetUserId();

            if (id.HasValue)
            {
                var existing = _context.Budgets
                    .FirstOrDefault(b => b.BudgetId == id.Value && b.UserId == userId);

                if (existing == null)
                    return NotFound();

                return View(existing);
            }

            var now = DateTime.Now;
            var model = new Budget
            {
                Year = year ?? now.Year,
                Month = month ?? now.Month
            };

            return View(model);
        }

        // POST: /Budget/SetBudget
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetBudget(Budget budget)
        {
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            var userId = GetUserId();

            if (ModelState.IsValid)
            {
                try
                {
                    if (budget.BudgetId == 0)
                    {
                        budget.UserId = userId;
                        _context.Budgets.Add(budget);
                    }
                    else
                    {
                        var existing = _context.Budgets
                            .FirstOrDefault(b => b.BudgetId == budget.BudgetId && b.UserId == userId);

                        if (existing == null)
                            return NotFound();

                        existing.Year = budget.Year;
                        existing.Month = budget.Month;
                        existing.PlannedAmount = budget.PlannedAmount;
                    }

                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving budget");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the budget.");
                }
            }

            return View(budget);
        }

        // GET: /Budget/ViewBudget?year=2025&month=12
        public IActionResult ViewBudget(int? year, int? month)
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

            var categoryBreakdown = _context.Transactions
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

            var vm = new MonthlyBudgetSummaryViewModel
            {
                Year = y,
                Month = m,
                PlannedAmount = budget?.PlannedAmount ?? 0m,
                Incomes = salaries + variableIncomes,
                Expenses = expenses,
                CategoryBreakdown = categoryBreakdown
            };

            return View(vm);
        }
    }
}


