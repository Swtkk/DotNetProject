using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Utility;

namespace WebApplication1.Controllers;

// [Authorize(Roles = SD.Role_Moderator)]
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Wyświetla listę kategorii
    public IActionResult Index(int pageNumber = 1)
    {
        var categories = _context.Categories
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * SD.PageSize)
            .Take(SD.PageSize)
            .ToList();

        int totalCategories = _context.Categories.Count();
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCategories / (double)SD.PageSize);

        return View(categories);
    }

    // GET: Wyświetla formularz dodawania kategorii
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }


    // POST: Zapisuje nową kategorię w bazie danych
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index)); 
        }

        return View(category);
    }

    public IActionResult Details(int id, int pageNumber = 1)
    {
        // Sprawdzenie, czy kategoria istnieje
        var category = _context.Categories
            .FirstOrDefault(c => c.CategoryId == id);

        if (category == null) return NotFound();

        // Pobieranie postów danej kategorii z paginacją
        var pinnedPosts = _context.Posts
            .Where(p => p.CategoryId == id && p.IsPinned)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * SD.PageSize)
            .Take(SD.PageSize)
            .ToList();

        var unpinnedPosts = _context.Posts
            .Where(p => p.CategoryId == id &&!p.IsPinned)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * SD.PageSize)
            .Take(SD.PageSize)
            .ToList();

        var allPosts = pinnedPosts.Concat(unpinnedPosts).ToList();
        
        int totalPosts = _context.Posts.Count(p => p.CategoryId == id && !p.IsPinned);
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)SD.PageSize);
        ViewBag.Category = category;

        return View(allPosts);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);
        if (category == null) return NotFound();

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category category)
    {
        if (!ModelState.IsValid) return View(category);

        var existingCategory = _context.Categories.Find(category.CategoryId);
        if (existingCategory == null) return NotFound();

        existingCategory.Name = category.Name;
        _context.SaveChanges();
        // return RedirectToAction("Index", "Category", new {id = category.CategoryId});
        return RedirectToAction(nameof(Index));
    }


  
}