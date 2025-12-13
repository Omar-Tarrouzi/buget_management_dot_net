using App_de_gestion_de_buget_version2.Data;
using App_de_gestion_de_buget_version2.Models;
using App_de_gestion_de_buget_version2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MongoDB.Bson;

namespace App_de_gestion_de_buget_version2.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ApplicationDbContext context, ILogger<TransactionController> logger)
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

        private void PopulateCategories(string userId, string? selectedCategoryId = null)
        {
            var categories = _context.Categories
                                     .Where(c => c.UserId == userId)
                                     .OrderBy(c => c.Nom)
                                     .ToList();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Nom", selectedCategoryId);
        }

        // GET: /Transaction
        public IActionResult Index()
        {
            var userId = GetUserId();

            // Get user's wallet first
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);
            if (wallet == null)
            {
                return View(new TransactionIndexViewModel
                {
                    Transactions = new List<Transaction>(),
                    CategoryBreakdown = new List<CategoryBreakdownItem>(),
                    TotalExpenses = 0,
                    TotalIncomes = 0
                });
            }

            // Get transactions for this wallet
            var transactions = _context.Transactions
                .Where(t => t.WalletId == wallet.WalletId)
                .OrderByDescending(t => t.Date)
                .ToList();

            // Manually load categories
            var categoryIds = transactions.Where(t => t.CategoryId != null).Select(t => t.CategoryId).Distinct().ToList();
            var categories = _context.Categories.Where(c => categoryIds.Contains(c.CategoryId)).ToList();
            
            // Assign navigation properties
            foreach (var transaction in transactions)
            {
                transaction.Wallet = wallet;
                if (transaction.CategoryId != null)
                {
                    transaction.Category = categories.FirstOrDefault(c => c.CategoryId == transaction.CategoryId);
                }
            }

            // Calculate category breakdown for expenses
            var categoryBreakdown = transactions
                .Where(t => t.Type == TransactionType.Expense && t.Category != null)
                .GroupBy(t => t.Category!.Nom)
                .Select(g => new CategoryBreakdownItem
                {
                    CategoryName = g.Key,
                    Total = g.Sum(t => t.Montant)
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            var totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Montant);

            var totalIncomes = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Montant);

            var viewModel = new TransactionIndexViewModel
            {
                Transactions = transactions,
                CategoryBreakdown = categoryBreakdown,
                TotalExpenses = totalExpenses,
                TotalIncomes = totalIncomes
            };

            return View(viewModel);
        }

        // GET: /Transaction/Details/5
        public IActionResult Details(string id)
        {
            var userId = GetUserId();

            var transaction = _context.Transactions.FirstOrDefault(t => t.TransactionId == id);
            if (transaction == null)
                return NotFound();

            // Manually load wallet and verify ownership
            var wallet = _context.Wallets.FirstOrDefault(w => w.WalletId == transaction.WalletId);
            if (wallet == null || wallet.UserId != userId)
                return NotFound();

            transaction.Wallet = wallet;

            // Manually load category if exists
            if (transaction.CategoryId != null)
            {
                transaction.Category = _context.Categories.FirstOrDefault(c => c.CategoryId == transaction.CategoryId);
            }

            return View(transaction);
        }

        // GET: /Transaction/Create
        public IActionResult Create()
        {
            var userId = GetUserId();
            PopulateCategories(userId);

            var model = new Transaction
            {
                Date = DateTime.Today,
                Type = TransactionType.Expense
            };

            return View(model);
        }

        // POST: /Transaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Transaction transaction)
        {
            // Remove navigation properties from validation
            ModelState.Remove("Wallet");
            ModelState.Remove("WalletId"); // Manually assigned below
            ModelState.Remove("Category");
            ModelState.Remove("Wallet.Balance");

            var userId = GetUserId();
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);

            if (wallet == null)
            {
                ModelState.AddModelError(string.Empty, "You need to create a wallet before creating transactions.");
            }
            else
            {
                transaction.WalletId = wallet.WalletId;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (wallet == null)
                    {
                        throw new Exception("Wallet not found for current user.");
                    }

                    // Affecter le wallet
                    transaction.TransactionId = ObjectId.GenerateNewId().ToString();
                    _context.Transactions.Add(transaction);

                    // Mettre à jour le solde en fonction du type
                    wallet.Balance ??= 0m;
                    if (transaction.Type == TransactionType.Income)
                    {
                        wallet.Balance += transaction.Montant;
                    }
                    else
                    {
                        wallet.Balance -= transaction.Montant;
                    }

                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating transaction");
                    ModelState.AddModelError(string.Empty, $"An error occurred while saving the transaction: {ex.Message} {(ex.InnerException != null ? " - " + ex.InnerException.Message : "")}");
                }
            }

            PopulateCategories(userId, transaction.CategoryId);
            return View(transaction);
        }

        // GET: /Transaction/Edit/5
        public IActionResult Edit(string id)
        {
            var userId = GetUserId();

            var transaction = _context.Transactions.FirstOrDefault(t => t.TransactionId == id);
            if (transaction == null)
                return NotFound();

            // Manually load wallet and verify ownership
            var wallet = _context.Wallets.FirstOrDefault(w => w.WalletId == transaction.WalletId);
            if (wallet == null || wallet.UserId != userId)
                return NotFound();

            transaction.Wallet = wallet;

            PopulateCategories(userId, transaction.CategoryId);
            return View(transaction);
        }

        // POST: /Transaction/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Transaction input)
        {
            ModelState.Remove("Wallet");
            ModelState.Remove("WalletId"); // Manually assigned/verified
            ModelState.Remove("Category");
            ModelState.Remove("Wallet.Balance");

            var userId = GetUserId();

            var transaction = _context.Transactions.FirstOrDefault(t => t.TransactionId == input.TransactionId);
            if (transaction == null)
                return NotFound();

            // Manually load wallet and verify ownership
            var wallet = _context.Wallets.FirstOrDefault(w => w.WalletId == transaction.WalletId);
            if (wallet == null || wallet.UserId != userId)
                return NotFound();

            transaction.Wallet = wallet;

            if (ModelState.IsValid)
            {
                try
                {
                    if (transaction.Wallet == null)
                    {
                        throw new Exception("Wallet not found for transaction.");
                    }

                    // Annuler l'effet de l'ancienne transaction sur le solde
                    transaction.Wallet.Balance ??= 0m;
                    if (transaction.Type == TransactionType.Income)
                    {
                        transaction.Wallet.Balance -= transaction.Montant;
                    }
                    else
                    {
                        transaction.Wallet.Balance += transaction.Montant;
                    }

                    // Mettre à jour la transaction
                    transaction.Date = input.Date;
                    transaction.Description = input.Description;
                    transaction.Montant = input.Montant;
                    transaction.CategoryId = input.CategoryId;
                    transaction.Type = input.Type;

                    // Appliquer le nouvel effet sur le solde
                    if (transaction.Type == TransactionType.Income)
                    {
                        transaction.Wallet.Balance += transaction.Montant;
                    }
                    else
                    {
                        transaction.Wallet.Balance -= transaction.Montant;
                    }

                    _context.Update(transaction);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error editing transaction");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the transaction.");
                }
            }

            PopulateCategories(userId, input.CategoryId);
            return View(input);
        }

        // GET: /Transaction/Delete/5
        public IActionResult Delete(string id)
        {
            var userId = GetUserId();

            var transaction = _context.Transactions.FirstOrDefault(t => t.TransactionId == id);
            if (transaction == null)
                return NotFound();

            // Manually load wallet and verify ownership
            var wallet = _context.Wallets.FirstOrDefault(w => w.WalletId == transaction.WalletId);
            if (wallet == null || wallet.UserId != userId)
                return NotFound();

            transaction.Wallet = wallet;

            // Manually load category if exists
            if (transaction.CategoryId != null)
            {
                transaction.Category = _context.Categories.FirstOrDefault(c => c.CategoryId == transaction.CategoryId);
            }

            return View(transaction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var userId = GetUserId();

            var transaction = _context.Transactions.FirstOrDefault(t => t.TransactionId == id);
            if (transaction == null)
                return NotFound();

            // Manually load wallet and verify ownership
            var wallet = _context.Wallets.FirstOrDefault(w => w.WalletId == transaction.WalletId);
            if (wallet == null || wallet.UserId != userId)
                return NotFound();

            transaction.Wallet = wallet;

            if (transaction == null)
                return NotFound();

            try
            {
                if (transaction.Wallet == null)
                {
                    throw new Exception("Wallet not found for transaction.");
                }

                // Annuler l'effet de la transaction sur le solde
                transaction.Wallet.Balance ??= 0m;
                if (transaction.Type == TransactionType.Income)
                {
                    transaction.Wallet.Balance -= transaction.Montant;
                }
                else
                {
                    transaction.Wallet.Balance += transaction.Montant;
                }

                _context.Transactions.Remove(transaction);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the transaction.");
                return View(transaction);
            }
        }
    }
}


