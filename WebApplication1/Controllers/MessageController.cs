using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

public class MessageController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}