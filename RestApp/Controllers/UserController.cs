using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using restapp.Dal;
using restapp.Models;
using System.Reflection.Metadata.Ecma335;

namespace restapp.Controllers
{
    public class UserController : Controller
    {
        //created a context class object for this controller. because it uses context class
        private readonly RestContext _context;
        //created a constructor to usercontroller. the memory for the dbcontext object is created 
        //only when the controller is used/ called
        public UserController(RestContext context)
        {
            _context = context;
        }
        public IActionResult Login() // returns login.cshtml (view)
        {
            return View();
        }
       


        public IActionResult Logout() // removing the user logged in details from the session
        {
            HttpContext.Session.Remove("loggedinuser");
            HttpContext.Session.Remove("loggedinuserRole");

            return RedirectToAction("Index", "Home");
        }

        //after the user clicks the signin button in login view then
        //the data will be sent to validateUser action to get validated 
        [HttpPost]
        public IActionResult ValidateUser(UserLogin ul)
        {
            //if the submitted data is not valid we ask the user to
            //enter again the data, by providing the login page again
            //here ModelState represents the annotations that we declared in model class
            if (!ModelState.IsValid)
            {
                return View("Login");
            }
            //here the data provided by the user in login page are
            //compared to the data that is present in users database(User) table.
            User? x = _context.users.FirstOrDefault(u => u.UserId.ToLower() == ul.UserId.ToLower() && u.Password == ul.Password);
            if (x == null) // when no user found
            {
                ModelState.AddModelError(string.Empty, "Incorrect user id or password");
                return View("Login");
            }
            else
            {
                if (x.Status == true)
                {
                    //here status means , admin can close the activation of any user
                    //check for user's role type
                    Role r = _context.roles.Find(x.RoleId);
                    if (r != null)
                    {
                        if (r.RoleName == "Admin")
                        {
                            HttpContext.Session.SetString("loggedinuser", ul.UserId);
                            HttpContext.Session.SetString("loggedinuserRole", "Admin");
                            //index - action , admin - controller
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (r.RoleName == "Employee1")
                        {
                            HttpContext.Session.SetString("loggedinuser", ul.UserId);
                            HttpContext.Session.SetString("loggedinuserRole", "Employee1");
                            return RedirectToAction("Index", "Employee1");
                        }
                        else if (r.RoleName == "Employee2")
                        {
                            HttpContext.Session.SetString("loggedinuser", ul.UserId);
                            HttpContext.Session.SetString("loggedinuserRole", "Employee2");
                            return RedirectToAction("Index", "Employee2");
                        }
                        else if (r.RoleName == "User")
                        {
                            HttpContext.Session.SetString("loggedinuser", ul.UserId);
                            HttpContext.Session.SetString("loggedinuserRole", "User");
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return View("Login");
                        }
                    }
                    else
                    {
                        return View("Login");
                    }
                }
                else
                {
                    return View("Login"); // user status is not activate by admin 
                }
            }
            //return View();
        }
        public IActionResult Register()
        {
            return View();
        }

        // 1. NEW: [HttpPost] Action to handle user registration
        [HttpPost]
        public IActionResult RegisterUser(User u)
        {
            // Check if the submitted data is valid based on model annotations
            if (!ModelState.IsValid)
            {
                return View("Register", u); // Return to Register view with data and errors
            }

            try
            {
                // A. Check if UserId already exists to prevent duplicate
                if (_context.users.Any(user => user.UserId.ToLower() == u.UserId.ToLower()))
                {
                    ModelState.AddModelError("UserId", "This Username is already taken.");
                    return View("Register", u);
                }

                // B. Set the default RoleId for a new 'User'
                // You must get the ID of the 'User' role from your database. Assuming RoleId = 4 for 'User' based on your logic flow.
                // Replace 4 with the actual ID of the 'User' role from your 'roles' table.
                Role? defaultRole = _context.roles.FirstOrDefault(r => r.RoleName == "User");
                u.RoleId = defaultRole?.RoleId ?? 0; // Set RoleId, handle null if role not found

                // C. Status should be set here if not handled by the view (it is currently taken from the view, but good practice is to control it here)
                // u.Status = false; // Example: Set to false for admin approval

                _context.users.Add(u);
                _context.SaveChanges();

                // D. Set the success message in TempData before redirecting
                TempData["SuccessMessage"] = "Registration successful! Please login with your new credentials.";

                // E. Redirect to the Login page
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Log the error (best practice) and return to the registration form with a generic error
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");
                // Optional: You might want to remove the password field from the model before returning to the view for security.
                return View("Register", u);
            }
        }
    }
}