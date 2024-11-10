using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class PostController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 20;
    public PostController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Wyświetla formularz tworzenia wątku
    public IActionResult Create(int categoryId)
    {
        ViewBag.CategoryId = categoryId;
        return View();
    }

    // POST: Tworzy nowy wątek w kategorii
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Post post)
    {
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage); // Wypisuje błędy walidacji w konsoli
            }
            ViewBag.CategoryId = post.CategoryId;
            return View(post);
        }

        post.CreatedAt = DateTime.Now;
        _context.Posts.Add(post);
        _context.SaveChanges();
        return RedirectToAction("Details", "Category", new { id = post.CategoryId });
    }

    // GET: Wyświetla szczegóły wątku i listę wiadomości
    public IActionResult Details(int id)
    {
        var post = _context.Posts
            .Where(p => p.PostId == id)
            .Select(p => new
            {
                Post = p,
                Messages = p.Messages.OrderBy(m => m.CreatedAt).ToList()
            })
            .FirstOrDefault();

        if (post == null) return NotFound();

        ViewBag.Post = post.Post;
        return View(post.Messages);
    }
}