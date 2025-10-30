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
        // Inside UserController.cs

        [HttpPost]
        public IActionResult ValidateUser(UserLogin ul)
        {
            if (!ModelState.IsValid)
            {
                // Return to Login view, showing any model validation errors (e.g., empty fields)
                return View("Login", ul); // Pass the model back to retain input
            }

            // 1. Check if the user ID exists (case-insensitive check)
            User? user = _context.users.FirstOrDefault(u => u.UserId.ToLower() == ul.UserId.ToLower());

            // Check if the user was found
            if (user == null)
            {
                // User ID not found
                ModelState.AddModelError(string.Empty, "The username you entered is not registered.");
                return View("Login", ul);
            }

            // 2. Check the password (since the user was found)
            // NOTE: Your current code uses plain text password comparison (u.Password == ul.Password).
            // In a real application, this should be replaced with a secure password HASH verification.
            if (user.Password != ul.Password)
            {
                // Password found but doesn't match the one entered
                ModelState.AddModelError(string.Empty, "The password you entered is incorrect."); // Specific error for wrong password
                return View("Login", ul);
            }

            // 3. User found and password matches (Authentication Successful)
            // The rest of your original logic for status and role checking goes here.

            if (user.Status == true)
            {
                // Check for user's role type
                Role r = _context.roles.Find(user.RoleId);
                if (r != null)
                {
                    if (r.RoleName == "Admin")
                    {
                        HttpContext.Session.SetString("loggedinuser", ul.UserId);
                        HttpContext.Session.SetString("loggedinuserRole", "Admin");
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (r.RoleName == "Employee1")
                    {
                        HttpContext.Session.SetString("loggedinuser", ul.UserId);
                        HttpContext.Session.SetString("loggedinuserRole", "Employee1");
                        return RedirectToAction("Index", "Employee1");
                    }
                    // ... (Continue with other roles) ...
                    else if (r.RoleName == "User")
                    {
                        HttpContext.Session.SetString("loggedinuser", ul.UserId);
                        HttpContext.Session.SetString("loggedinuserRole", "User");
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Fallback for an unrecognized role
                        ModelState.AddModelError(string.Empty, "Your account has an invalid role configuration.");
                        return View("Login", ul);
                    }
                }
                else
                {
                    // Fallback if role ID is set but role object is missing
                    ModelState.AddModelError(string.Empty, "Could not verify user role.");
                    return View("Login", ul);
                }
            }
            else
            {
                // User status is not active (admin approval pending/denied)
                ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact the administrator.");
                return View("Login", ul);
            }
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