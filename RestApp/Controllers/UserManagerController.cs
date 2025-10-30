using Microsoft.AspNetCore.Mvc;

// restapp.Controllers.UserManagerController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restapp.Dal;
using restapp.Models;

namespace restapp.Controllers
{
    public class UserManagerController : Controller
    {
        private readonly RestContext _context;

        public UserManagerController(RestContext context)
        {
            _context = context;
        }

        // GET: UserManager
        public async Task<IActionResult> Index()
        {
            // --- (1) Authorization Check ---
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedinuserRole != "Admin")
            {
                return RedirectToAction("Login", "User");
            }

            // --- (2) Fetch Users ---
            // Fetch users along with their roles (using lowercase 'role' from your model)
            var users = await _context.users
                                      .Include(u => u.role)
                                      .OrderBy(u => u.Id)
                                      .ToListAsync();

            return View(users); // Pass the list of users to the view
        }

        // POST: UserManager/ToggleStatus/5 (Uses the int Id primary key)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id) // Use int Id for primary key
        {
            // --- (1) Authorization Check ---
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedinuserRole != "Admin")
            {
                return RedirectToAction("Login", "User");
            }

            // --- (2) Find the User ---
            var user = await _context.users.FindAsync(id); // Find using int Id
            if (user == null)
            {
                return NotFound();
            }

            // --- (3) Toggle the existing Status property ---
            user.Status = !user.Status;

            // --- (4) Save Changes ---
            _context.users.Update(user);
            await _context.SaveChangesAsync();

            // Set a message (optional)
            TempData["Message"] = $"User {user.UserId} has been {(user.Status ? "Activated" : "Deactivated")}.";

            return RedirectToAction(nameof(Index));
        }

        // GET: UserManager/OrderHistory/5 (Placeholder for Order History)
        public IActionResult OrderHistory(int id)
        {
            // Logic to fetch orders for the user ID
            ViewBag.UserId = id;
            return View();
        }
    }
}