using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using Confluent.Kafka;
namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
       
    }
  
    public IActionResult Index()
    {
        ViewBag.MemeUrl = "/images/default-meme.jpg"; 

        var categories = _context.Categories.OrderBy(c => c.Name).ToList();
        ViewBag.TotalCategories = _context.Categories.Count();
        ViewBag.TotalPosts = _context.Posts.Count();
        ViewBag.TotalUsers = _context.Users.Count();
        ViewBag.TotalMessages = _context.Messages.Count();

        return View(categories);
    }

    [HttpGet("api/getMeme")]
    public IActionResult GetMemeFromKafka()
    {
        
        var memeUrl = KafkaConsumerService.GetLastMemeUrl();
    
        if (string.IsNullOrEmpty(memeUrl))
        {
            Console.WriteLine("⚠️ Brak memów w Kafka. Ustawiam domyślny obraz.");
            memeUrl = "\\Images\\userAvatar.png"; // Możesz podać link do domyślnego obrazka
        }

        return Json(new { url = memeUrl });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
}