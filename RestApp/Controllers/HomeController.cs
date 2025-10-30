using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restapp.Dal;
using restapp.Models;
using System.Diagnostics;

namespace restapp.Controllers
{
    public class HomeController : Controller
    {
       
        // Keep the logger field
        private readonly ILogger<HomeController> _logger;

        // Add the database context field
        private readonly RestContext _context;

        // 💡 UPDATED CONSTRUCTOR: Takes both ILogger and RestContext
        public HomeController(ILogger<HomeController> logger, RestContext context)
        {
            _logger = logger;
            _context = context; // Store the context for database operations
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Menu()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
    }
}
