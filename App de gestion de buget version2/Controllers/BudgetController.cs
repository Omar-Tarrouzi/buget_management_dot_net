using App_de_gestion_de_buget_version2.Data;
using App_de_gestion_de_buget_version2.Models;
using App_de_gestion_de_buget_version2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MongoDB.Bson;

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
        public IActionResult SetBudget(string? id, int? year, int? month)
        {
            var userId = GetUserId();
            var now = DateTime.Now;
            int targetYear = year ?? now.Year;
            int targetMonth = month ?? now.Month;

            Budget budget;

            if (!string.IsNullOrEmpty(id))
            {
                budget = _context.Budgets.FirstOrDefault(b => b.BudgetId == id && b.UserId == userId);
                if (budget == null) return NotFound();
            }
            else
            {
                // Check if budget exists for this year/month to avoid duplicate creation if user just navigates via link
                 var existing = _context.Budgets.FirstOrDefault(b => b.UserId == userId && b.Year == targetYear && b.Month == targetMonth);
                 if (existing != null)
                 {
                     budget = existing;
                 }
                 else 
                 {
                    budget = new Budget
                    {
                        Year = targetYear,
                        Month = targetMonth
                    };
                 }
            }

            // Load Categories
            var categories = _context.Categories.Where(c => c.UserId == userId).OrderBy(c => c.Nom).ToList();
            
            // Load existing category budgets
            var categoryBudgets = _context.CategoryBudgets
                .Where(cb => cb.UserId == userId && cb.Year == budget.Year && cb.Month == budget.Month)
                .ToList();

            var viewModel = new SetBudgetViewModel
            {
                Budget = budget,
                CategoryBudgets = categories.Select(c => {
                    var cb = categoryBudgets.FirstOrDefault(x => x.CategoryId == c.CategoryId);
                    return new CategoryBudgetInputModel
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.Nom,
                        Limit = cb?.MaxAmount > 0 ? cb.MaxAmount : null
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /Budget/SetBudget
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetBudget(SetBudgetViewModel model)
        {
            // Manual validation cleanup not strictly necessary if utilizing viewmodel binding correctly
            // but keeping safety checks
            ModelState.Remove("Budget.User");
            ModelState.Remove("Budget.UserId");

            var userId = GetUserId();
            var budget = model.Budget;

            if (ModelState.IsValid)
            {
                try
                {
                    // Save or Update Budget
                    if (string.IsNullOrEmpty(budget.BudgetId))
                    {
                        // Check for duplicate
                        var existing = _context.Budgets.FirstOrDefault(b => b.UserId == userId && b.Year == budget.Year && b.Month == budget.Month);
                        if (existing != null)
                        {
                            budget.BudgetId = existing.BudgetId;
                            existing.PlannedAmount = budget.PlannedAmount;
                        }
                        else
                        {
                            budget.UserId = userId;
                            budget.BudgetId = ObjectId.GenerateNewId().ToString();
                            _context.Budgets.Add(budget);
                        }
                    }
                    else
                    {
                        var existing = _context.Budgets.FirstOrDefault(b => b.BudgetId == budget.BudgetId && b.UserId == userId);
                        if (existing == null) return NotFound();

                        existing.Year = budget.Year;
                        existing.Month = budget.Month;
                        existing.PlannedAmount = budget.PlannedAmount;
                    }

                    // Save Category Budgets
                    if (model.CategoryBudgets != null)
                    {
                        foreach (var catInput in model.CategoryBudgets)
                        {
                            var existingCB = _context.CategoryBudgets.FirstOrDefault(cb =>
                                cb.UserId == userId &&
                                cb.CategoryId == catInput.CategoryId &&
                                cb.Year == budget.Year &&
                                cb.Month == budget.Month);

                            if (catInput.Limit.HasValue && catInput.Limit.Value > 0)
                            {
                                if (existingCB == null)
                                {
                                    var newCB = new CategoryBudget
                                    {
                                        CategoryBudgetId = ObjectId.GenerateNewId().ToString(),
                                        UserId = userId,
                                        CategoryId = catInput.CategoryId,
                                        Year = budget.Year,
                                        Month = budget.Month,
                                        MaxAmount = catInput.Limit.Value
                                    };
                                    _context.CategoryBudgets.Add(newCB);
                                }
                                else
                                {
                                    existingCB.MaxAmount = catInput.Limit.Value;
                                }
                            }
                            else
                            {
                                if (existingCB != null)
                                {
                                    _context.CategoryBudgets.Remove(existingCB);
                                }
                            }
                        }
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
            
            // Reload categories on failure
             var categories = _context.Categories.Where(c => c.UserId == userId).OrderBy(c => c.Nom).ToList();
             // Reconstruct input list if needed or trust binding. 
             // Ideally we should re-populate names if they were lost, but hidden inputs handle that.
             // Just to be safe, ensuring names are present:
             foreach(var cb in model.CategoryBudgets) {
                 var cat = categories.FirstOrDefault(c => c.CategoryId == cb.CategoryId);
                 if(cat != null) cb.CategoryName = cat.Nom;
             }

            return View(model);
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

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);
            if (wallet == null)
            {
                return View(new MonthlyBudgetSummaryViewModel
                {
                    Year = y,
                    Month = m,
                    PlannedAmount = budget?.PlannedAmount ?? 0m,
                    Incomes = 0m,
                    Expenses = 0m,
                    CategoryBreakdown = new List<CategoryBreakdownItem>()
                });
            }

            var salaries = _context.Salaries
                .Where(s => s.WalletId == wallet.WalletId &&
                            s.Payday.Year == y &&
                            s.Payday.Month == m)
                .ToList()
                .Sum(s => s.Montant);

            var variableIncomes = _context.Transactions
                .Where(t => t.WalletId == wallet.WalletId &&
                            t.Type == TransactionType.Income &&
                            t.Date.HasValue &&
                            t.Date.Value.Year == y &&
                            t.Date.Value.Month == m)
                .ToList()
                .Sum(t => t.Montant);

            var expenses = _context.Transactions
                .Where(t => t.WalletId == wallet.WalletId &&
                            t.Type == TransactionType.Expense &&
                            t.Date.HasValue &&
                            t.Date.Value.Year == y &&
                            t.Date.Value.Month == m)
                .ToList()
                .Sum(t => t.Montant);

            var transactions = _context.Transactions
                .Where(t => t.WalletId == wallet.WalletId &&
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

            // Load Category Budgets (Limits)
            var categoryLimits = _context.CategoryBudgets
                .Where(cb => cb.UserId == userId && cb.Year == y && cb.Month == m)
                .ToList();

            var categoryBreakdown = transactions
                .GroupBy(t => t.CategoryId)
                .Select(g => 
                {
                    var catId = g.Key;
                    var catName = g.FirstOrDefault(t => t.Category != null)?.Category?.Nom ?? "Sans catégorie";
                    var limit = categoryLimits.FirstOrDefault(cb => cb.CategoryId == catId)?.MaxAmount;

                    return new CategoryBreakdownItem
                    {
                        CategoryName = catName,
                        Total = g.Sum(t => t.Montant),
                        Limit = limit
                    };
                })
                .OrderByDescending(c => c.Total)
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


