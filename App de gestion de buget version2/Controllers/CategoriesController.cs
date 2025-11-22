using App_de_gestion_de_buget_version2.Data;
using Microsoft.AspNetCore.Mvc; // Ton namespace
using App_de_gestion_de_buget_version2.Models; // Tes modèles

public class CategoriesController : Controller
{
    private readonly BudgetContext _context;

    public CategoriesController(BudgetContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var categories = _context.Categories.ToList();
        return View(categories);
    }
    // GET: Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Create
    [HttpPost]
    public IActionResult Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(category);
    }

    public IActionResult Edit(int id)
    {
        var c = _context.Categories.Find(id);
        return View(c);
    }

    [HttpPost]
    public IActionResult Edit(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Update(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(category);
    }

    public IActionResult Delete(int id)
    {
        var c = _context.Categories.Find(id);
        return View(c);
    }

    [HttpPost]
    public IActionResult DeleteConfirmed(int id)
    {
        var c = _context.Categories.Find(id);
        _context.Categories.Remove(c);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }



}
