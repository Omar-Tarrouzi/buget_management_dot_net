using App_de_gestion_de_buget_version2.Data;
using App_de_gestion_de_buget_version2.Models;
using App_de_gestion_de_buget_version2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace App_de_gestion_de_buget_version2.Controllers
{
    [Authorize]
    public class DataExportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CsvService _csvService;
        private readonly UserManager<IdentityUser> _userManager;

        public DataExportController(ApplicationDbContext context, CsvService csvService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _csvService = csvService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Export(string entityType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            byte[] fileBytes;
            string fileName;

            switch (entityType.ToLower())
            {
                case "transaction":
                    var transactions = await _context.Transactions
                        .Include(t => t.Category)
                        .Include(t => t.Wallet)
                        .Where(t => t.Wallet.UserId == user.Id)
                        .Select(t => new TransactionImportDto
                        {
                            Date = t.Date,
                            Description = t.Description,
                            Montant = t.Montant,
                            Type = t.Type.ToString(),
                            Category = t.Category != null ? t.Category.Nom : ""
                        })
                        .ToListAsync();
                    fileBytes = _csvService.ExportToCsv(transactions);
                    fileName = "transactions.csv";
                    break;

                case "category":
                    var categories = await _context.Categories
                        .Where(c => c.UserId == user.Id)
                        .ToListAsync();
                    fileBytes = _csvService.ExportToCsv(categories);
                    fileName = "categories.csv";
                    break;

                case "wallet":
                    var wallets = await _context.Wallets
                        .Where(w => w.UserId == user.Id)
                        .ToListAsync();
                    fileBytes = _csvService.ExportToCsv(wallets);
                    fileName = "wallets.csv";
                    break;
                
                case "budget":
                    var budgets = await _context.Budgets
                        .Where(b => b.UserId == user.Id)
                        .ToListAsync();
                    fileBytes = _csvService.ExportToCsv(budgets);
                    fileName = "budgets.csv";
                    break;

                 case "goal":
                    var goals = await _context.Goals
                        .Where(g => g.UserId == user.Id)
                        .ToListAsync();
                    fileBytes = _csvService.ExportToCsv(goals);
                    fileName = "goals.csv";
                    break;

                default:
                    return BadRequest("Entity type not supported");
            }

            return File(fileBytes, "text/csv", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> Import(string entityType, IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            using var stream = file.OpenReadStream();

            try
            {
                switch (entityType.ToLower())
                {
                    case "transaction":
                        // Use the DTO to handle string-based categories and types
                        var dtos = _csvService.ImportFromCsv<TransactionImportDto>(stream);
                        int newCategoriesCount = 0;

                        foreach (var dto in dtos)
                        {
                            var t = new Transaction
                            {
                                Date = dto.Date,
                                Description = dto.Description,
                                Montant = dto.Montant,
                                TransactionId = 0
                            };

                            // 1. Handling Type (Dépense/Income string parsing)
                            string typeStr = dto.Type?.Trim().ToLower() ?? "";
                            if (typeStr == "income" || typeStr == "revenu" || typeStr == "1")
                                t.Type = TransactionType.Income;
                            else // Default to Expense
                                t.Type = TransactionType.Expense;

                            // 2. Validate Wallet (or assign default)
                            var defaultWallet = _context.Wallets.FirstOrDefault(w => w.UserId == user.Id);
                            if (defaultWallet != null)
                            {
                                t.WalletId = defaultWallet.WalletId;
                            }
                            else
                            {
                                // No wallet exists? Need to create one default or skip
                                continue; 
                            }

                            // 3. Category Intelligence
                            string categoryName = dto.Category ?? dto.Catégorie ?? dto.Categorie;
                            
                            if (!string.IsNullOrWhiteSpace(categoryName))
                            {
                                // A. Format Name (Capitalize first letter)
                                categoryName = char.ToUpper(categoryName[0]) + categoryName.Substring(1).ToLower();

                                // B. Check if exists
                                var existingCategory = _context.Categories
                                    .FirstOrDefault(c => c.UserId == user.Id && c.Nom.ToLower() == categoryName.ToLower());

                                if (existingCategory != null)
                                {
                                    t.CategoryId = existingCategory.CategoryId;
                                }
                                else
                                {
                                    // C. Create New Category
                                    var newCat = new Category
                                    {
                                        Nom = categoryName,
                                        UserId = user.Id,
                                        User = user
                                    };
                                    _context.Categories.Add(newCat);
                                    await _context.SaveChangesAsync(); // Save immediately to get ID
                                    t.CategoryId = newCat.CategoryId;
                                    newCategoriesCount++;
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(t.Description))
                            {
                                // D. Fallback: Deduce from Description
                                var pastTransaction = _context.Transactions
                                    .Where(tr => tr.Wallet.UserId == user.Id && tr.Description.ToLower() == t.Description.ToLower() && tr.CategoryId != null)
                                    .OrderByDescending(tr => tr.Date)
                                    .FirstOrDefault();

                                if (pastTransaction != null)
                                {
                                    t.CategoryId = pastTransaction.CategoryId;
                                }
                            }

                            _context.Transactions.Add(t);
                        }
                        if(newCategoriesCount > 0) TempData["Info"] = $"{newCategoriesCount} new categories created.";
                        break;

                    case "category":
                        var categories = _csvService.ImportFromCsv<Category>(stream);
                        foreach (var c in categories)
                        {
                            c.CategoryId = 0;
                            c.UserId = user.Id;
                            c.User = user;
                            if (!_context.Categories.Any(cat => cat.UserId == user.Id && cat.Nom == c.Nom))
                            {
                                _context.Categories.Add(c);
                            }
                        }
                        break;

                    case "wallet":
                        var wallets = _csvService.ImportFromCsv<Wallet>(stream);
                        foreach (var w in wallets)
                        {
                            w.WalletId = 0;
                            w.UserId = user.Id;
                            w.User = user;
                            _context.Wallets.Add(w);
                        }
                        break;
                    
                    case "goal":
                         var goals = _csvService.ImportFromCsv<Goal>(stream);
                         foreach(var g in goals)
                         {
                             g.GoalId = 0;
                             g.UserId = user.Id;
                             g.User = user;
                             _context.Goals.Add(g);
                         }
                         break;

                   case "budget":
                         var budgets = _csvService.ImportFromCsv<Budget>(stream);
                         foreach(var b in budgets)
                         {
                             b.BudgetId = 0;
                             b.UserId = user.Id;
                             b.User = user;
                             _context.Budgets.Add(b);
                         }
                         break;

                    default:
                        return BadRequest("Entity type not supported for import");
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Import of {entityType} successful!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error importing: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }

    // DTO for Import/Export ensuring flexibility with CSV headers
    public class TransactionImportDto
    {
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
        public decimal Montant { get; set; }
        
        // Supports "Expense" or "Dépense"
        public string? Type { get; set; } 

        // Supports multiple header names for maximum compatibility
        public string? Category { get; set; }
        public string? Catégorie { get; set; }
        public string? Categorie { get; set; }
    }
}
