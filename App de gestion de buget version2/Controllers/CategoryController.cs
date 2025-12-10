using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using App_de_gestion_de_buget_version2.Data;
using App_de_gestion_de_buget_version2.Models;
using System.Linq;

namespace App_de_gestion_de_buget_version2.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ApplicationDbContext context, ILogger<CategoryController> logger)
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

        public IActionResult Index()
        {
            var userId = GetUserId();
            var categories = _context.Categories
                                    .Where(c => c.UserId == userId)
                                    .ToList();
            return View(categories);
        }

        public IActionResult Create()
        {
            // return an empty model to the view so tag helpers render correctly
            return View(new Category());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    category.UserId = GetUserId();
                    _context.Categories.Add(category);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving category");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the category.");
                }
            }
            else
            {
                // Log model state errors to help debugging
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        _logger.LogWarning("ModelState error for {Key}: {Error}", kvp.Key, error.ErrorMessage);
                    }
                }
            }

            return View(category);
        }


        public IActionResult Edit(int id)
        {
            var userId = GetUserId();
            var category = _context.Categories
                                  .FirstOrDefault(c => c.CategoryId == id && c.UserId == userId);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            // Ne pas valider les propriétés de navigation / UserId fournies par le client
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            var userId = GetUserId();
            var existingCategory = _context.Categories
                                          .FirstOrDefault(c => c.CategoryId == category.CategoryId && c.UserId == userId);

            if (existingCategory == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingCategory.Nom = category.Nom;
                _context.Categories.Update(existingCategory);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public IActionResult Delete(int id)
        {
            var userId = GetUserId();
            var category = _context.Categories
                                  .FirstOrDefault(c => c.CategoryId == id && c.UserId == userId);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            var category = _context.Categories
                                  .FirstOrDefault(c => c.CategoryId == id && c.UserId == userId);

            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}