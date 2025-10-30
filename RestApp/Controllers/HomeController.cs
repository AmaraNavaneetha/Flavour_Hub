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
        // 💡 NEW ACTION TO HANDLE USER SEARCH REQUESTS
        public async Task<IActionResult> SearchResults(string searchString)
        {
            // Store the search string to pre-fill the search bar (optional but good UX)
            ViewData["CurrentFilter"] = searchString;

            // Start with all available food items, including Category and ItemType details
            var foodItemsQuery = _context.fooditems
                                    .Include(f => f.category)
                                    .Include(f => f.itemType)
                                    .AsQueryable();

            // Apply the search filter if a search string is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                // Convert search string to lower case once for case-insensitive comparison
                string lowerSearch = searchString.ToLower();

                // Filter items where the name OR the category name contains the search term
                foodItemsQuery = foodItemsQuery.Where(f =>
                    f.ItemName.ToLower().Contains(lowerSearch) ||
                    (f.category != null && f.category.CategoryName.ToLower().Contains(lowerSearch))
                );
            }

            // Execute the query and return the list of items
            List<FoodItem> searchResults = await foodItemsQuery
                                            // Optional: Order the results for consistency
                                            .OrderBy(f => f.ItemName)
                                            .ToListAsync();

            // Determine which view to return. The bycategory view should work well for displaying items.
            return View("ByCategory", searchResults);
        }
    }
}
