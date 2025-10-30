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
        // Inside UserController.cs

        // Action to display the user's profile and pre-fill the edit form.
        public IActionResult Profile()
        {
            // 1. Authorization Check (already added in UserMenu, but good practice here too)
            string? loggedInUserId = HttpContext.Session.GetString("loggedinuser");
            if (loggedInUserId == null)
            {
                return RedirectToAction("Login", "User");
            }

            // 2. Fetch User Data
            // Fetch the full User object from the database using the UserId stored in the session.
            User? user = _context.users.FirstOrDefault(u => u.UserId.ToLower() == loggedInUserId.ToLower());

            if (user == null)
            {
                // Handle case where session is set but user doesn't exist (e.g., deleted by admin)
                HttpContext.Session.Remove("loggedinuser");
                HttpContext.Session.Remove("loggedinuserRole");
                return RedirectToAction("Login", "User");
            }

            // 3. Pass the user object to the View
            return View(user);
        }

        // Action to handle the form submission when the user clicks 'Save Changes'.
        [HttpPost]
        public IActionResult Profile(User updatedUser) // The form binds to the User model
        {
            // 1. Authorization Check
            string? loggedInUserId = HttpContext.Session.GetString("loggedinuser");
            if (loggedInUserId == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Ensure the submitted ID matches the logged-in user's ID
            if (updatedUser.UserId.ToLower() != loggedInUserId.ToLower())
            {
                // Security check: prevent editing another user's profile
                return Forbid();
            }

            // 2. Model Validation
            // Remove RoleId and Status from validation, as they are often handled server-side or not part of the form.
            ModelState.Remove("RoleId");
            ModelState.Remove("Status");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors in the form.";
                return View(updatedUser); // Return with validation errors
            }

            try
            {
                // 3. Fetch the original user record from the database
                User? originalUser = _context.users.Find(updatedUser.Id); // Assuming 'Id' is the primary key. If not, use FirstOrDefault.

                if (originalUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Logout", "User");
                }

                // 4. Update ONLY the editable properties
                originalUser.FirstName = updatedUser.FirstName;
                originalUser.LastName = updatedUser.LastName;
                // originalUser.UserId is generally not editable
                originalUser.Mobile = updatedUser.Mobile;
                originalUser.Email = updatedUser.Email;

                // Only update password if the user entered a new one (requires a change in the model/view)
                if (!string.IsNullOrEmpty(updatedUser.Password))
                {
                    originalUser.Password = updatedUser.Password;
                }

                // 5. Save Changes
                _context.users.Update(originalUser);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = "An unexpected error occurred while saving your details.";
                return View(updatedUser);
            }
        }
    }
}