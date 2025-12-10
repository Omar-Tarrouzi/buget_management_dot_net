using App_de_gestion_de_buget_version2.Data;
using App_de_gestion_de_buget_version2.Models;
using App_de_gestion_de_buget_version2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        private void PopulateCategories(string userId, int? selectedCategoryId = null)
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

            var transactions = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Wallet)
                .Where(t => t.Wallet.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToList();

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
        public IActionResult Details(int id)
        {
            var userId = GetUserId();

            var transaction = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Wallet)
                .FirstOrDefault(t => t.TransactionId == id && t.Wallet.UserId == userId);

            if (transaction == null)
                return NotFound();

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
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the transaction.");
                }
            }

            PopulateCategories(userId, transaction.CategoryId);
            return View(transaction);
        }

        // GET: /Transaction/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = GetUserId();

            var transaction = _context.Transactions
                .Include(t => t.Wallet)
                .FirstOrDefault(t => t.TransactionId == id && t.Wallet.UserId == userId);

            if (transaction == null)
                return NotFound();

            PopulateCategories(userId, transaction.CategoryId);
            return View(transaction);
        }

        // POST: /Transaction/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Transaction input)
        {
            ModelState.Remove("Wallet");
            ModelState.Remove("Category");
            ModelState.Remove("Wallet.Balance");

            var userId = GetUserId();

            var transaction = _context.Transactions
                .Include(t => t.Wallet)
                .FirstOrDefault(t => t.TransactionId == input.TransactionId && t.Wallet.UserId == userId);

            if (transaction == null)
                return NotFound();

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
        public IActionResult Delete(int id)
        {
            var userId = GetUserId();

            var transaction = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Wallet)
                .FirstOrDefault(t => t.TransactionId == id && t.Wallet.UserId == userId);

            if (transaction == null)
                return NotFound();

            return View(transaction);
        }

        // POST: /Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var userId = GetUserId();

            var transaction = _context.Transactions
                .Include(t => t.Wallet)
                .FirstOrDefault(t => t.TransactionId == id && t.Wallet.UserId == userId);

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


