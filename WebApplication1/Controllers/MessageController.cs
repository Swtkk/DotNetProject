using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class MessageController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public MessageController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Message message, List<IFormFile> Attachments)
    {
        var forbiddenWords = new List<string> { "zakazane1", "zakazane2" };
        if (forbiddenWords.Any(word => message.Content.Contains(word, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("", "Wiadomość zawiera zakazane słowa.");
            return RedirectToAction("Details", "Post", new { id = message.PostId });
        }

        message.CreatedAt = DateTime.Now;
        message.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Zapisz wiadomość
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Obsługa załączników
        if (Attachments != null && Attachments.Count > 0)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Attachments");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var file in Attachments)
            {
                if (file.Length > 2 * 1024 * 1024) continue; // Ignoruj za duże pliki

                var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var attachment = new Attachment
                {
                    FileName = file.FileName,
                    FilePath = Path.Combine("Attachments", fileName),
                    MessageId = message.MessageId
                };
                _context.Attachments.Add(attachment);
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", "Post", new { id = message.PostId });
    }

    [HttpGet]
    public IActionResult ReportedMessages()
    {
        var reportedMessages = _context.Messages
            .Include(m => m.User)
            .Include(m => m.Post)
            .Include(m => m.Attachments)
            .Where(m => m.IsReported)
            .ToList();

        return View(reportedMessages);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsNotReported(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null)
        {
            return NotFound();
        }

        // Ustaw zgłoszenie na "false"
        message.IsReported = false;
        await _context.SaveChangesAsync();

        // Przekieruj z powrotem do listy zgłoszonych wiadomości
        return RedirectToAction(nameof(ReportedMessages));
    }
    [HttpPost]
    public async Task<IActionResult> Report(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null)
        {
            return NotFound();
        }

        message.IsReported = true;
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Post", new { id = message.PostId });
    }

    // GET: Wyświetlanie formularza edycji wiadomości
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var message = _context.Messages
            .Include(m => m.Attachments)
            .FirstOrDefault(m => m.MessageId == id);

        if (message == null)
        {
            return NotFound();
        }
        Console.WriteLine($"MessageId: {message.MessageId}, PostId: {message.PostId}");
        return View(message);
    }

// POST: Aktualizacja wiadomości
    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(Message message, List<IFormFile> Attachments)
{
    // Znajdź istniejącą wiadomość w bazie danych
    var existingMessage = _context.Messages
        .Include(m => m.Attachments)
        .FirstOrDefault(m => m.MessageId == message.MessageId);

    if (existingMessage == null)
    {
        return NotFound();
    }

    if (!ModelState.IsValid)
    {
        // Przekaż istniejącą wiadomość, aby zachować jej stan
        foreach (var key in ModelState.Keys)
        {
            var state = ModelState[key];
            foreach (var error in state.Errors)
            {
                Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
            }
        }
        return View(existingMessage);
    }

    // Aktualizuj treść wiadomości
    existingMessage.Content = message.Content;

    // Jeśli są nowe załączniki, usuń stare
    if (Attachments != null && Attachments.Any())
    {
        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Attachments");

        // Usuń istniejące załączniki z serwera i bazy danych
        foreach (var attachment in existingMessage.Attachments)
        {
            var filePath = Path.Combine(uploadsFolder, attachment.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            _context.Attachments.Remove(attachment);
        }

        // Dodaj nowe załączniki
        foreach (var file in Attachments)
        {
            if (file.Length > 2 * 1024 * 1024) continue; // Ignoruj za duże pliki

            var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new Attachment
            {
                FileName = file.FileName,
                FilePath = Path.Combine("Attachments", fileName),
                MessageId = existingMessage.MessageId
            };

            _context.Attachments.Add(attachment);
        }
    }

    // Zapisz zmiany w bazie danych
    await _context.SaveChangesAsync();

    // Przekierowanie do szczegółów posta
    return RedirectToAction("Details", "Post", new { id = existingMessage.PostId });
}








// POST: Usuwanie wiadomości
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var message = await _context.Messages.Include(m => m.Attachments).FirstOrDefaultAsync(m => m.MessageId == id);
        if (message == null)
        {
            return NotFound();
        }

        // Usuń załączniki z serwera
        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Attachments");
        foreach (var attachment in message.Attachments)
        {
            var filePath = Path.Combine(uploadsFolder, attachment.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        // Usuń wiadomość i załączniki z bazy danych
        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Post", new { id = message.PostId });
    }
   



}