using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 12;
    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Wyświetla listę kategorii
    public IActionResult Index(int pageNumber = 1)
    {
        var categories = _context.Categories
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        int totalCategories = _context.Categories.Count();
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCategories / (double)PageSize);

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
            return RedirectToAction(nameof(Index)); // Przekierowanie do listy kategorii po dodaniu
        }

        return View(category);
    }
    
    public IActionResult Details(int id, int pageNumber = 1)
    {
        const int PageSize = 12;

        // Sprawdzenie, czy kategoria istnieje
        var category = _context.Categories
            .FirstOrDefault(c=> c.CategoryId ==id);

        if (category == null) return NotFound();

        // Pobieranie postów danej kategorii z paginacją
        var posts = _context.Posts
            .Where(p => p.CategoryId == id)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        int totalPosts = _context.Posts.Count(p => p.CategoryId == id);
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)PageSize);
        ViewBag.Category = category;

        return View(posts);
    }

}