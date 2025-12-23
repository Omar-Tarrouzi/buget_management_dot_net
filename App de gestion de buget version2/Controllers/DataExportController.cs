using App_de_gestion_de_buget_version2.Data;
using App_de_gestion_de_buget_version2.Models;
using App_de_gestion_de_buget_version2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.IO;

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
        public async Task<IActionResult> Export(string entityType, string format = "csv")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            byte[] fileBytes;
            string fileName;

            switch (entityType.ToLower())
            {
                case "transaction":
                    var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id);
                    if (wallet == null)
                    {
                        return BadRequest("No wallet found for user");
                    }

                    var transactions = await _context.Transactions
                        .Where(t => t.WalletId == wallet.WalletId)
                        .ToListAsync();

                    // Manually load categories
                    var categoryIds = transactions.Where(t => t.CategoryId != null).Select(t => t.CategoryId).Distinct().ToList();
                    var categories = await _context.Categories.Where(c => categoryIds.Contains(c.CategoryId)).ToListAsync();
                    
                    var transactionDtos = transactions.Select(t => new TransactionImportDto
                    {
                        Date = t.Date,
                        Description = t.Description,
                        Montant = t.Montant,
                        Type = t.Type.ToString(),
                        Category = t.CategoryId != null ? categories.FirstOrDefault(c => c.CategoryId == t.CategoryId)?.Nom ?? "" : ""
                    }).ToList();



                    if (format == "json")
                    {
                        var json = transactions.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson });
                        fileBytes = Encoding.UTF8.GetBytes(json);
                        fileName = "transactions.json";
                    }
                    else
                    {
                        fileBytes = _csvService.ExportToCsv(transactionDtos);
                        fileName = "transactions.csv";
                    }
                    break;

                case "category":
                    var categoriesForExport = await _context.Categories
                        .Where(c => c.UserId == user.Id)
                        .ToListAsync();
                    if (format == "json")
                    {
                         var json = categoriesForExport.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson });
                         fileBytes = Encoding.UTF8.GetBytes(json);
                         fileName = "categories.json";
                    }
                    else
                    {
                        fileBytes = _csvService.ExportToCsv(categoriesForExport);
                        fileName = "categories.csv";
                    }
                    break;

                case "wallet":
                    var wallets = await _context.Wallets
                        .Where(w => w.UserId == user.Id)
                        .ToListAsync();
                    if (format == "json")
                    {
                         var json = wallets.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson });
                         fileBytes = Encoding.UTF8.GetBytes(json);
                         fileName = "wallets.json";
                    }
                    else
                    {
                        fileBytes = _csvService.ExportToCsv(wallets);
                        fileName = "wallets.csv";
                    }
                    break;
                
                case "budget":
                    var budgets = await _context.Budgets
                        .Where(b => b.UserId == user.Id)
                        .ToListAsync();
                    if (format == "json") 
                    {
                         var json = budgets.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson });
                         fileBytes = Encoding.UTF8.GetBytes(json);
                         fileName = "budgets.json";
                    }
                    else
                    {
                        fileBytes = _csvService.ExportToCsv(budgets);
                        fileName = "budgets.csv";
                    }
                    break;

                 case "goal":
                    var goals = await _context.Goals
                        .Where(g => g.UserId == user.Id)
                        .ToListAsync();
                    if (format == "json")
                    {
                         var json = goals.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson });
                         fileBytes = Encoding.UTF8.GetBytes(json);
                         fileName = "goals.json";
                    }
                    else
                    {
                        fileBytes = _csvService.ExportToCsv(goals);
                        fileName = "goals.csv";
                    }
                    break;

                default:
                    return BadRequest("Entity type not supported");
            }

            if (format == "json")
                return File(fileBytes, "application/json", fileName);
            
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
                // Detect JSON by extension or content type, or try/catch logic
                bool isJson = file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

                if (isJson)
                {
                    string jsonContent;
                    using (var reader = new StreamReader(stream))
                    {
                        jsonContent = await reader.ReadToEndAsync();
                    }

                    switch (entityType.ToLower())
                    {
                         case "transaction":
                            var transactions = BsonSerializer.Deserialize<List<Transaction>>(jsonContent);
                            var defaultWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id);
                            // If user has no wallet, we can't really attach transactions easily without more logic.
                            // Assuming they might be restoring to a clean DB, maybe we should keep the wallet ID from JSON?
                            // For safety, let's keep original IDs to allow full restore, but ensure WalletId is valid if possible?
                            // If we are just viewing, importing implies "add to current".
                            
                            foreach(var t in transactions)
                            {
                                // If the transaction already has an ID, check if it exists
                                if (!string.IsNullOrEmpty(t.TransactionId))
                                {
                                    var exists = await _context.Transactions.AnyAsync(x => x.TransactionId == t.TransactionId);
                                    if(exists) continue; // Skip duplicates
                                }
                                else
                                {
                                    t.TransactionId = ObjectId.GenerateNewId().ToString();
                                }
                                
                                // Fix wallet link if needed? 
                                // If we import generic JSON, WalletId might be from another user.
                                // FORCE current user constraint:
                                // Find valid wallet
                                if (defaultWallet != null && (string.IsNullOrEmpty(t.WalletId) || !_context.Wallets.Any(w => w.WalletId == t.WalletId)))
                                {
                                     t.WalletId = defaultWallet.WalletId;
                                }

                                _context.Transactions.Add(t);
                            }
                            break;

                         case "category":
                            var cats = BsonSerializer.Deserialize<List<Category>>(jsonContent);
                            foreach(var c in cats)
                            {
                                c.UserId = user.Id; // Force ownership
                                if(!string.IsNullOrEmpty(c.CategoryId) && await _context.Categories.AnyAsync(x => x.CategoryId == c.CategoryId)) continue;
                                if(string.IsNullOrEmpty(c.CategoryId)) c.CategoryId = ObjectId.GenerateNewId().ToString();

                                _context.Categories.Add(c);
                            }
                            break;

                         case "wallet":
                            var walls = BsonSerializer.Deserialize<List<Wallet>>(jsonContent);
                            foreach(var w in walls)
                            {
                                w.UserId = user.Id;
                                if(!string.IsNullOrEmpty(w.WalletId) && await _context.Wallets.AnyAsync(x => x.WalletId == w.WalletId)) continue;
                                if(string.IsNullOrEmpty(w.WalletId)) w.WalletId = ObjectId.GenerateNewId().ToString();
                                _context.Wallets.Add(w);
                            }
                            break;

                        case "budget":
                            var buds = BsonSerializer.Deserialize<List<Budget>>(jsonContent);
                            foreach(var b in buds)
                            {
                                b.UserId = user.Id;
                                if(!string.IsNullOrEmpty(b.BudgetId) && await _context.Budgets.AnyAsync(x => x.BudgetId == b.BudgetId)) continue;
                                if(string.IsNullOrEmpty(b.BudgetId)) b.BudgetId = ObjectId.GenerateNewId().ToString();
                                _context.Budgets.Add(b);
                            }
                            break;

                        case "goal":
                            var gs = BsonSerializer.Deserialize<List<Goal>>(jsonContent);
                            foreach(var g in gs)
                            {
                                g.UserId = user.Id;
                                if(!string.IsNullOrEmpty(g.GoalId) && await _context.Goals.AnyAsync(x => x.GoalId == g.GoalId)) continue;
                                if(string.IsNullOrEmpty(g.GoalId)) g.GoalId = ObjectId.GenerateNewId().ToString();
                                _context.Goals.Add(g);
                            }
                            break;
                    }
                }
                else
                {
                    // CSV Logic
                    // Restore stream position for CSV reader?
                    // Stream was read to end. Need to reset.
                    // But CSV service takes stream.
                    // Better separate logic.
                }

                if (!isJson)
                {
                     // Reset stream for CSV reading
                     stream.Position = 0; 
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
                                TransactionId = ObjectId.GenerateNewId().ToString(),
                                Date = dto.Date,
                                Description = dto.Description,
                                Montant = dto.Montant
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
                                        UserId = user.Id
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
                                    .Where(tr => tr.WalletId == defaultWallet.WalletId && tr.Description.ToLower() == t.Description.ToLower() && tr.CategoryId != null)
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
                            c.UserId = user.Id;
                            if (!_context.Categories.Any(cat => cat.UserId == user.Id && cat.Nom == c.Nom))
                            {
                                if (string.IsNullOrEmpty(c.CategoryId)) c.CategoryId = ObjectId.GenerateNewId().ToString();
                                _context.Categories.Add(c);
                            }
                        }
                        break;

                    case "wallet":
                        var wallets = _csvService.ImportFromCsv<Wallet>(stream);
                        foreach (var w in wallets)
                        {
                            w.UserId = user.Id;
                            if (string.IsNullOrEmpty(w.WalletId)) w.WalletId = ObjectId.GenerateNewId().ToString();
                            _context.Wallets.Add(w);
                        }
                        break;
                    
                    case "goal":
                         var goals = _csvService.ImportFromCsv<Goal>(stream);
                         foreach(var g in goals)
                         {
                             g.UserId = user.Id;
                             if (string.IsNullOrEmpty(g.GoalId)) g.GoalId = ObjectId.GenerateNewId().ToString();
                             _context.Goals.Add(g);
                         }
                         break;

                   case "budget":
                         var budgets = _csvService.ImportFromCsv<Budget>(stream);
                         foreach(var b in budgets)
                         {
                             b.UserId = user.Id;
                             if (string.IsNullOrEmpty(b.BudgetId)) b.BudgetId = ObjectId.GenerateNewId().ToString();
                             _context.Budgets.Add(b);
                         }
                         break;

                    default:
                        return BadRequest("Entity type not supported for import");
                }
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
