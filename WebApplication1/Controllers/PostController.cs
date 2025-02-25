using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Utility;

namespace WebApplication1.Controllers;

public class PostController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public PostController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Wyświetla formularz tworzenia wątku
    public IActionResult Create(int categoryId)
    {
        ViewBag.CategoryId = categoryId;
        return View();
    }

    [HttpGet]
    public IActionResult Search(string query, int pageNumber = 1)
    {
        var postsQuery = _context.Posts.AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            postsQuery = postsQuery.Where(p => p.Title.Contains(query));
        }
        
        var totalPosts = postsQuery.Count();
        
        var posts = postsQuery
            .OrderByDescending(p=> p.CreatedAt)
            .Skip((pageNumber - 1) * SD.PageSize)
            .Take(SD.PageSize)
           .ToList();

        ViewBag.Query = query;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)SD.PageSize);
        
        return View(posts);
    }
    
    [HttpGet]
    public IActionResult SearchAjax(string query)
    {
        var posts = _context.Posts
            .Where(p => p.Title.Contains(query))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                PostId = p.PostId,
                Title = p.Title,
                CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        return Json(posts);
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
    public IActionResult Details(int id, int pageNumber =1)
    {
        int messageDisplay = 15;
        var post = _context.Posts
            .Include(p => p.Messages)
            .ThenInclude(m => m.Attachments)
            .Include(p => p.Messages)
            .ThenInclude((m => m.User))
            .FirstOrDefault(p => p.PostId == id);
        if (post == null) return NotFound();
        
        var messages = post.Messages
            .OrderBy(c=>c.CreatedAt)
            .Skip((pageNumber - 1) * messageDisplay)
            .Take(messageDisplay)
            .ToList();
        
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(post.Messages.Count / (double)messageDisplay);
        post.Messages = messages;
        
        return View(post);
    }


    [HttpGet]
    public IActionResult Edit(int id)
    {
        var post = _context.Posts.Find(id);
        if (post == null) return NotFound();

        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Post post)
    {
        if (!ModelState.IsValid)
        {
            return View(post);
        }

        var existingPost = _context.Posts.Find(post.PostId);
        if (existingPost == null) return NotFound();

        existingPost.Title = post.Title;
        existingPost.Content = post.Content;
        existingPost.IsPinned = post.IsPinned;
        _context.SaveChanges();
        return RedirectToAction("Details", "Category", new { id = post.CategoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var post = _context.Posts.Find(id);
        if (post == null) return NotFound();

        _context.Posts.Remove(post);
        _context.SaveChanges();
        return RedirectToAction("Details", "Category", new { id = post.CategoryId });
    }


    [HttpPost]
    public IActionResult TogglePin(int id)
    {
        var post = _context.Posts.Find(id);
        if (post == null) return NotFound();

        post.IsPinned = !post.IsPinned; // Odwrócenie flagi przypięcia
        _context.SaveChanges();

        return RedirectToAction("Details", "Category", new { id = post.CategoryId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAttachment(int AttachmentId)
    {
        var attachment = await _context.Attachments
            .Include(a => a.Message) // Pobierz powiązaną wiadomość
            .ThenInclude(m => m.Post) // Pobierz powiązany post
            .FirstOrDefaultAsync(a => a.AttachmentId == AttachmentId);

        if (attachment == null)
        {
            return NotFound();
        }

        // Usuń plik załącznika z serwera
        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Attachments");
        var filePath = Path.Combine(uploadsFolder, attachment.FilePath);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        // Pobierz PostId przed usunięciem załącznika
        int postId = attachment.Message.PostId;

        // Usuń załącznik z bazy danych
        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();

        // Przekierowanie do szczegółów posta
        return RedirectToAction("Details", "Post", new { id = postId });
    }
}