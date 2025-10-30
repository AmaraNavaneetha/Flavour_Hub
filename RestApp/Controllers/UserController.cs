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
        // Inside UserController.cs, add this new action:

        

        public IActionResult Cart()
        {
            // 1. Authorization Check (Optional, but good practice)
            string? loggedInUserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedInUserRole != "User")
            {
                // If not logged in as a user, redirect them (e.g., to login or home)
                return RedirectToAction("Index", "Home");
            }

            // 2. Retrieve Cart Data from Session
            List<CartItem> cart = new List<CartItem>();
            string? cartJson = HttpContext.Session.GetString("ShoppingCart");

            if (!string.IsNullOrEmpty(cartJson))
            {
                // Deserialize the JSON string back into a List<CartItem>
                // Ensure Newtonsoft.Json is imported if not already.
                cart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItem>>(cartJson) ?? new List<CartItem>();
            }

            // 3. Return the List of CartItems to a View named 'Cart.cshtml'
            return View(cart);
        }

        // --- HELPER METHODS FOR CART MANAGEMENT (Add these inside UserController) ---

        // Helper to get cart from session
        private List<CartItem> GetCartFromSession()
        {
            // Uses the session key "ShoppingCart"
            string? cartJson = HttpContext.Session.GetString("ShoppingCart");

            // Checks if the session string is null/empty and deserializes if found
            return string.IsNullOrEmpty(cartJson)
                   ? new List<CartItem>()
                   : Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        // Helper to save cart to session
        private void SaveCartToSession(List<CartItem> cart)
        {
            string updatedCartJson = Newtonsoft.Json.JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("ShoppingCart", updatedCartJson);
        }
        // Inside UserController.cs, add/replace these methods:

        // --- Helper to determine the redirect location ---
        private string GetRedirectAction(string categoryName)
        {
            // Assuming you are using a 'ByCategory' action in the Home controller
            // or you can redirect back to a generic Menu action.
            // Adjust this to match your actual menu action.
            return Url.Action("ByCategory", "Home", new { category = categoryName }) ?? Url.Action("Menu", "Home");
        }


        [HttpPost]
        public IActionResult AddToCartSubmit(int id, string returnUrl) // 'id' is FoodItem.Id
        {
            // You should modify your view to pass the current category name or return URL
            // so you can redirect back to the correct page after updating the cart.

            // 1. Authorization & Fetch Item (Same as before)
            // ...
            var foodItem = _context.fooditems.FirstOrDefault(f => f.ItemId == id);
            // ... (Error handling if item not found) ...

            // 2. Get/Update Cart (Same logic as before)
            List<CartItem> cart = GetCartFromSession();
            CartItem? existingItem = cart.FirstOrDefault(item => item.FoodItemId == id);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var newCartItem = new CartItem
                {
                    FoodItemId = foodItem.ItemId,
                    Name = foodItem.ItemName,
                    Price = foodItem.SellingPrice,
                    Quantity = 1
                };
                cart.Add(newCartItem);
            }

            // 3. Save and Redirect
            SaveCartToSession(cart);
            TempData["SuccessMessage"] = $"{foodItem.ItemName} added to cart!";

            // Redirect back to the page the user was on
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Menu", "Home"); // Fallback
        }


        [HttpPost]
        public IActionResult RemoveFromCartSubmit(int id, string returnUrl)
        {
            // 1. Authorization (Check the role if needed, though for cart operations, 
            // it’s often okay as long as the session cart is protected)
            // ... your existing authorization logic here if any ...

            // 2. Get/Update Cart
            List<CartItem> cart = GetCartFromSession();
            CartItem? existingItem = cart.FirstOrDefault(item => item.FoodItemId == id);

            if (existingItem != null)
            {
                // DECREASE QUANTITY BY ONE
                existingItem.Quantity--;

                if (existingItem.Quantity <= 0)
                {
                    // REMOVE ITEM if quantity hits zero
                    cart.Remove(existingItem);
                    TempData["SuccessMessage"] = $"{existingItem.Name} has been completely removed from your cart.";
                }
                else
                {
                    // QUANTITY DECREASED
                    TempData["SuccessMessage"] = $"{existingItem.Name} quantity reduced to {existingItem.Quantity}.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Could not find item in cart to reduce quantity.";
            }

            // 3. Save and Redirect
            SaveCartToSession(cart);

            // Redirect back to the page the user was on (Menu or Cart)
            if (!string.IsNullOrEmpty(returnUrl))
            {
                // This is important for handling quantity changes on the Cart page itself.
                return Redirect(returnUrl);
            }
            return RedirectToAction("Menu", "Home"); // Fallback
        }
        // NEW: Action to remove a specific item completely from the cart list, regardless of quantity.
        [HttpPost]
        public IActionResult RemoveItemFullyFromCart(int id)
        {
            // 1. Authorization Check (Ensures only a User can delete items)
            string? loggedInUserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedInUserRole != "User")
            {
                TempData["ErrorMessage"] = "Unauthorized action.";
                return RedirectToAction("Login", "User");
            }

            // 2. Get Cart from Session
            List<CartItem> cart = GetCartFromSession();
            CartItem? existingItem = cart.FirstOrDefault(item => item.FoodItemId == id);
            string itemName = "Item";

            if (existingItem != null)
            {
                itemName = existingItem.Name;

                // Remove the item entirely from the cart list
                cart.Remove(existingItem);

                TempData["SuccessMessage"] = $"{itemName} has been completely removed from your cart.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not find item in cart to remove.";
            }

            // 3. Save the updated Cart and Redirect
            SaveCartToSession(cart);
            return RedirectToAction("Cart"); // Redirect back to the Cart view
        }
    }
}