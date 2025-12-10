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
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WalletController> _logger;

        public WalletController(ApplicationDbContext context, ILogger<WalletController> logger)
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

        // GET: /Wallet
        public IActionResult Index()
        {
            var userId = GetUserId();
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);

            if (wallet == null)
            {
                return View((Wallet?)null);
            }

            // Process recurring incomes (add automatically every 30 days)
            ProcessRecurringIncomes(userId, wallet);

            var now = DateTime.Now;
            var currentYear = now.Year;
            var currentMonth = now.Month;

            // Calculate current month expenses
            var currentMonthExpenses = _context.Transactions
                .Include(t => t.Wallet)
                .Where(t => t.Wallet.UserId == userId &&
                            t.Type == TransactionType.Expense &&
                            t.Date.HasValue &&
                            t.Date.Value.Year == currentYear &&
                            t.Date.Value.Month == currentMonth)
                .Sum(t => (decimal?)t.Montant) ?? 0m;

            // Calculate current month incomes
            var salaries = _context.Salaries
                .Include(s => s.Wallet)
                .Where(s => s.Wallet.UserId == userId &&
                            s.Payday.Year == currentYear &&
                            s.Payday.Month == currentMonth)
                .Sum(s => (decimal?)s.Montant) ?? 0m;

            var variableIncomes = _context.Transactions
                .Include(t => t.Wallet)
                .Where(t => t.Wallet.UserId == userId &&
                            t.Type == TransactionType.Income &&
                            t.Date.HasValue &&
                            t.Date.Value.Year == currentYear &&
                            t.Date.Value.Month == currentMonth)
                .Sum(t => (decimal?)t.Montant) ?? 0m;

            var currentMonthIncomes = salaries + variableIncomes;

            // Get current month budget
            var currentMonthBudget = _context.Budgets
                .Where(b => b.UserId == userId && b.Year == currentYear && b.Month == currentMonth)
                .Select(b => b.PlannedAmount)
                .FirstOrDefault();

            // Calculate budget usage percentage
            var budgetUsagePercent = currentMonthBudget > 0 
                ? (currentMonthExpenses / currentMonthBudget) * 100 
                : 0;

            // Determine budget health status
            string budgetHealthStatus;
            if (budgetUsagePercent <= 75)
                budgetHealthStatus = "good";
            else if (budgetUsagePercent <= 90)
                budgetHealthStatus = "warning";
            else
                budgetHealthStatus = "danger";

            // Calculate savings rate
            var savingsRate = currentMonthIncomes > 0 
                ? ((currentMonthIncomes - currentMonthExpenses) / currentMonthIncomes) * 100 
                : 0;

            // Count months with budgets and months over budget
            var allBudgets = _context.Budgets
                .Where(b => b.UserId == userId)
                .ToList();

            var monthsWithBudget = allBudgets.Count;
            var monthsOverBudget = 0;

            foreach (var budget in allBudgets)
            {
                var monthExpenses = _context.Transactions
                    .Include(t => t.Wallet)
                    .Where(t => t.Wallet.UserId == userId &&
                                t.Type == TransactionType.Expense &&
                                t.Date.HasValue &&
                                t.Date.Value.Year == budget.Year &&
                                t.Date.Value.Month == budget.Month)
                    .Sum(t => (decimal?)t.Montant) ?? 0m;

                if (monthExpenses > budget.PlannedAmount)
                {
                    monthsOverBudget++;
                }
            }

            // Calculate category breakdown for current month expenses
            var categoryBreakdown = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Wallet)
                .Where(t => t.Wallet.UserId == userId &&
                            t.Type == TransactionType.Expense &&
                            t.Date.HasValue &&
                            t.Date.Value.Year == currentYear &&
                            t.Date.Value.Month == currentMonth)
                .GroupBy(t => t.Category != null ? t.Category.Nom : "Sans catégorie")
                .Select(g => new CategoryBreakdownItem
                {
                    CategoryName = g.Key,
                    Total = g.Sum(t => t.Montant)
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            // Check category budgets and generate alerts
            var categoryBudgetAlerts = CheckCategoryBudgets(userId, currentYear, currentMonth, categoryBreakdown);
            
            // Update budget health status if any category is over budget
            if (categoryBudgetAlerts.Any())
            {
                budgetHealthStatus = "danger"; // Bad credit if any category exceeds budget
            }

            var dashboard = new WalletDashboardViewModel
            {
                Balance = wallet.Balance ?? 0m,
                CurrentMonthExpenses = currentMonthExpenses,
                CurrentMonthIncomes = currentMonthIncomes,
                CurrentMonthBudget = currentMonthBudget,
                BudgetUsagePercent = budgetUsagePercent,
                BudgetHealthStatus = budgetHealthStatus,
                SavingsRate = savingsRate,
                MonthsWithBudget = monthsWithBudget,
                MonthsOverBudget = monthsOverBudget,
                CategoryBreakdown = categoryBreakdown,
                CategoryBudgetAlerts = categoryBudgetAlerts
            };

            ViewBag.Wallet = wallet;
            return View(dashboard);
        }

        // GET: /Wallet/Create
        public IActionResult Create()
        {
            var userId = GetUserId();
            var existing = _context.Wallets.FirstOrDefault(w => w.UserId == userId);
            if (existing != null)
            {
                // Un seul wallet par utilisateur → aller à l’édition
                return RedirectToAction(nameof(Edit), new { id = existing.WalletId });
            }

            var model = new Wallet
            {
                Balance = 0m
            };

            return View(model);
        }

        // POST: /Wallet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Wallet wallet)
        {
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            var userId = GetUserId();

            // S’assurer qu’il n’y a pas déjà un wallet
            var existing = _context.Wallets.FirstOrDefault(w => w.UserId == userId);
            if (existing != null)
            {
                return RedirectToAction(nameof(Edit), new { id = existing.WalletId });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    wallet.UserId = userId;
                    _context.Wallets.Add(wallet);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating wallet");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the wallet.");
                }
            }

            return View(wallet);
        }

        // GET: /Wallet/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = GetUserId();
            var wallet = _context.Wallets.FirstOrDefault(w => w.WalletId == id && w.UserId == userId);

            if (wallet == null)
                return NotFound();

            return View(wallet);
        }

        // POST: /Wallet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Wallet wallet)
        {
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            var userId = GetUserId();
            var existing = _context.Wallets.FirstOrDefault(w => w.WalletId == wallet.WalletId && w.UserId == userId);

            if (existing == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    existing.Balance = wallet.Balance;
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating wallet");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the wallet.");
                }
            }

            return View(wallet);
        }

        private void ProcessRecurringIncomes(string userId, Wallet wallet)
        {
            // Check if RecurringIncomes table exists - if not, skip processing
            try
            {
                var recurringIncomes = _context.RecurringIncomes
                    .Where(ri => ri.UserId == userId && ri.WalletId == wallet.WalletId)
                    .ToList();

                var now = DateTime.Now;

                foreach (var recurringIncome in recurringIncomes)
            {
                var lastProcessed = recurringIncome.LastProcessedDate ?? recurringIncome.StartDate;
                var daysSinceLastProcessed = (now - lastProcessed).TotalDays;

                // Process if 30 days or more have passed
                if (daysSinceLastProcessed >= 30)
                {
                    // Calculate how many periods to process
                    var periodsToProcess = (int)(daysSinceLastProcessed / 30);

                    for (int i = 0; i < periodsToProcess; i++)
                    {
                        var processDate = lastProcessed.AddDays(30 * (i + 1));
                        
                        // Create transaction for recurring income
                        var transaction = new Transaction
                        {
                            Date = processDate,
                            Description = recurringIncome.Description,
                            Montant = recurringIncome.Montant,
                            Type = TransactionType.Income,
                            WalletId = wallet.WalletId,
                            CategoryId = null
                        };

                        _context.Transactions.Add(transaction);

                        // Update wallet balance
                        wallet.Balance = (wallet.Balance ?? 0m) + recurringIncome.Montant;
                    }

                    // Update last processed date
                    recurringIncome.LastProcessedDate = now;
                }
            }

                if (recurringIncomes.Any())
                {
                    _context.SaveChanges();
                }
            }
            catch
            {
                // Table doesn't exist yet or error accessing it, skip processing
                // This will be resolved once the migration is applied
            }
        }

        private List<CategoryBudgetAlert> CheckCategoryBudgets(string userId, int year, int month, List<CategoryBreakdownItem> categoryBreakdown)
        {
            var alerts = new List<CategoryBudgetAlert>();

            // Check if CategoryBudgets table exists - if not, return empty list
            try
            {
                // Get all category budgets for current month
                var categoryBudgets = _context.CategoryBudgets
                    .Include(cb => cb.Category)
                    .Where(cb => cb.UserId == userId && cb.Year == year && cb.Month == month)
                    .ToList();

                foreach (var categoryBudget in categoryBudgets)
                {
                    var categorySpending = categoryBreakdown
                        .FirstOrDefault(c => c.CategoryName == categoryBudget.Category.Nom);

                    if (categorySpending != null && categorySpending.Total > categoryBudget.MaxAmount)
                    {
                        var overAmount = categorySpending.Total - categoryBudget.MaxAmount;
                        var overPercentage = (overAmount / categoryBudget.MaxAmount) * 100;

                        alerts.Add(new CategoryBudgetAlert
                        {
                            CategoryName = categoryBudget.Category.Nom,
                            BudgetAmount = categoryBudget.MaxAmount,
                            SpentAmount = categorySpending.Total,
                            OverAmount = overAmount,
                            OverPercentage = overPercentage
                        });
                    }
                }
            }
            catch
            {
                // Table doesn't exist yet or error accessing it, return empty list
                // This will be resolved once the migration is applied
            }

            return alerts;
        }
    }
}


