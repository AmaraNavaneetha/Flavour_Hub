using Microsoft.AspNetCore.Mvc;
using restapp.Dal;

namespace restapp.Controllers
{
    public class AdminController : Controller
    {
        private readonly RestContext _context;

        // Constructor to inject the DbContext
        public AdminController(RestContext context)
        {
            _context = context;
        }
        public IActionResult Index() // returns Index.cshtml + _LayoutAdmin.cshtml
        {
            
        //get values from session
        string loggedInUser = HttpContext.Session.GetString("loggedinuser");
         string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if(loggedInUser != null && loggedinuserRole =="Admin")
            {
                ViewBag.loggedInUserId  = loggedInUser;

                // Fetch the counts and store them in the ViewBag
                ViewBag.TotalCategories = _context.categories.Count();
                ViewBag.TotalFoodItems = _context.fooditems.Count();

                // Assuming you have a DbSet<User> named 'users'
                ViewBag.TotalUsers = _context.users.Count();
                // ADD THIS NEW LINE TO FETCH ORDER COUNT
              //  ViewBag.TotalOrders = _context.orders.Count();
             
                ViewBag.TotalRevenue = "N/A";
                return View(); // returns Index.cshtml + _LayoutAdmin.cshtml
            }
            else 
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
 
        }
    }
}
