using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

public class Post : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
    
}