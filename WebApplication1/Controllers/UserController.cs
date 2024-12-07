using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Utility;

namespace WebApplication1.Controllers;

public class UserController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Wyświetlanie listy użytkowników
    public IActionResult Index(int pageNumber = 1)
    {
        var users = _context.Users
            .Select(u => new User
            {
                Id = u.Id,
                UserName = u.UserName,
                Role = _context.Roles.FirstOrDefault(r =>
                    _context.UserRoles.FirstOrDefault(ur => ur.UserId == u.Id).RoleId == r.Id).Name
            })
            .OrderBy(c => c.UserName)
            .Skip((pageNumber - 1) * SD.PageSize)
            .Take(SD.PageSize)
            .ToList();
        int totalUsers = _context.Users.Count();
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)SD.PageSize);

        return View(users);
    }

    // GET: Szczegóły użytkownika
    public IActionResult Details(string id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // GET: Edytowanie użytkownika
    public IActionResult Edit(string id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        // Pobierz rolę użytkownika
        var userRoleId = _context.UserRoles.FirstOrDefault(ur => ur.UserId == user.Id)?.RoleId;
        if (userRoleId != null)
        {
            user.Role = _context.Roles.FirstOrDefault(r => r.Id == userRoleId)?.Name;
        }

        // Przygotuj listę ról do wyboru w widoku
        ViewBag.Roles = _context.Roles.Select(r => new SelectListItem
        {
            Value = r.Name,
            Text = r.Name
        }).ToList();

        return View(user);
    }

  
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(User user)
    {

        if (!ModelState.IsValid)
        {
            // Wyświetl błędy walidacji w konsoli
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Błąd: {error.ErrorMessage}");
            }

            // Ponowne załadowanie listy ról w przypadku błędu
            ViewBag.Roles = _context.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();
            return View(user);
        }

        var existingUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser == null)
        {
            Console.WriteLine("Nie znaleziono użytkownika w bazie danych.");
            return NotFound();
        }

        // Zaktualizuj nazwę użytkownika
        existingUser.UserName = user.UserName;

        // Zaktualizuj rolę
        var userRole = _context.UserRoles.FirstOrDefault(ur => ur.UserId == user.Id);
        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
        }

        var newRole = _context.Roles.FirstOrDefault(r => r.Name == user.Role);
        if (newRole != null)
        {
            _context.UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = newRole.Id
            });
        }
        else
        {
            Console.WriteLine("Nie znaleziono nowej roli w bazie danych.");
            ModelState.AddModelError("", "Nie znaleziono nowej roli w bazie danych.");
            ViewBag.Roles = _context.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();
            return View(user);
        }

        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }


    // GET: Usuwanie użytkownika
    public IActionResult Delete(string id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // POST: Usuwanie użytkownika
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(string id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
}