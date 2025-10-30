using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using restapp.Dal;
using restapp.Models;
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore; // Needed for SaveChangesAsync
using System.Security.Cryptography.X509Certificates;

namespace restapp.Controllers
{
    public class UserController : Controller
    {
        private readonly RestContext _context;

        public UserController(RestContext context)
        {
            _context = context;
        }

        // --- HELPER METHODS ---

        // NEW: Helper to get the logged-in user ID from the session
        private string? GetUserId()
        {
            // The session key is "loggedinuser" as per your login logic
            return HttpContext.Session.GetString("loggedinuser");
        }

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

        // --- AUTHENTICATION ACTIONS ---

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

        [HttpPost]
        public IActionResult ValidateUser(UserLogin ul)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", ul);
            }

            User? user = _context.users.FirstOrDefault(u => u.UserId.ToLower() == ul.UserId.ToLower());

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "The username you entered is not registered.");
                return View("Login", ul);
            }

            if (user.Password != ul.Password)
            {
                ModelState.AddModelError(string.Empty, "The password you entered is incorrect.");
                return View("Login", ul);
            }

            if (user.Status == true)
            {
                // Find the role based on the RoleId
                Role? r = _context.roles.Find(user.RoleId);

                if (r != null)
                {
                    HttpContext.Session.SetString("loggedinuser", ul.UserId);
                    HttpContext.Session.SetString("loggedinuserRole", r.RoleName); // Store the actual role name

                    if (r.RoleName == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (r.RoleName == "Employee1")
                    {
                        return RedirectToAction("Index", "Employee1");
                    }
                    else if (r.RoleName == "User")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Your account has an unrecognized role configuration.");
                        return View("Login", ul);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Could not verify user role.");
                    return View("Login", ul);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact the administrator.");
                return View("Login", ul);
            }
        }

        // --- REGISTRATION & PROFILE ACTIONS (Omitted for brevity, assuming they are correct) ---
        public IActionResult Register() { return View(); }
        [HttpPost]
        public IActionResult RegisterUser(User u)
        {
            // ... (Your RegisterUser implementation) ...
            if (!ModelState.IsValid) return View("Register", u);

            try
            {
                if (_context.users.Any(user => user.UserId.ToLower() == u.UserId.ToLower()))
                {
                    ModelState.AddModelError("UserId", "This Username is already taken.");
                    return View("Register", u);
                }

                Role? defaultRole = _context.roles.FirstOrDefault(r => r.RoleName == "User");
                u.RoleId = defaultRole?.RoleId ?? 0;

                _context.users.Add(u);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful! Please login with your new credentials.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");
                return View("Register", u);
            }
        }
        public IActionResult Profile()
        {
            string? loggedInUserId = HttpContext.Session.GetString("loggedinuser");
            if (loggedInUserId == null) return RedirectToAction("Login", "User");

            User? user = _context.users.FirstOrDefault(u => u.UserId.ToLower() == loggedInUserId.ToLower());
            if (user == null) { HttpContext.Session.Clear(); return RedirectToAction("Login", "User"); }

            return View(user);
        }
        [HttpPost]
        public IActionResult Profile(User updatedUser)
        {
            string? loggedInUserId = HttpContext.Session.GetString("loggedinuser");
            if (loggedInUserId == null) return RedirectToAction("Login", "User");

            if (updatedUser.UserId.ToLower() != loggedInUserId.ToLower()) return Forbid();

            ModelState.Remove("RoleId");
            ModelState.Remove("Status");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors in the form.";
                return View(updatedUser);
            }

            try
            {
                User? originalUser = _context.users.Find(updatedUser.Id);

                if (originalUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Logout", "User");
                }

                originalUser.FirstName = updatedUser.FirstName;
                originalUser.LastName = updatedUser.LastName;
                originalUser.Mobile = updatedUser.Mobile;
                originalUser.Email = updatedUser.Email;

                if (!string.IsNullOrEmpty(updatedUser.Password))
                {
                    originalUser.Password = updatedUser.Password;
                }

                _context.users.Update(originalUser);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while saving your details.";
                return View(updatedUser);
            }
        }

        // --- CART ACTIONS ---

        public IActionResult Cart()
        {
            string? loggedInUserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedInUserRole != "User")
            {
                return RedirectToAction("Index", "Home");
            }

            List<CartItem> cart = GetCartFromSession();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCartSubmit(int id, string returnUrl) // 'id' is FoodItem.Id
        {
            // Fetch Item
            var foodItem = _context.fooditems.FirstOrDefault(f => f.ItemId == id);
            if (foodItem == null) { TempData["ErrorMessage"] = "Item not found."; return RedirectToAction("Menu", "Home"); }

            // Get/Update Cart
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

            // Save and Redirect
            SaveCartToSession(cart);
            TempData["SuccessMessage"] = $"{foodItem.ItemName} added to cart!";

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Menu", "Home"); // Fallback
        }


        [HttpPost]
        public IActionResult RemoveFromCartSubmit(int id, string returnUrl)
        {
            List<CartItem> cart = GetCartFromSession();
            CartItem? existingItem = cart.FirstOrDefault(item => item.FoodItemId == id);

            if (existingItem != null)
            {
                existingItem.Quantity--;

                if (existingItem.Quantity <= 0)
                {
                    cart.Remove(existingItem);
                    TempData["SuccessMessage"] = $"{existingItem.Name} has been completely removed from your cart.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"{existingItem.Name} quantity reduced to {existingItem.Quantity}.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Could not find item in cart to reduce quantity.";
            }

            SaveCartToSession(cart);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Menu", "Home"); // Fallback
        }

        [HttpPost]
        public IActionResult RemoveItemFullyFromCart(int id)
        {
            string? loggedInUserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedInUserRole != "User")
            {
                TempData["ErrorMessage"] = "Unauthorized action.";
                return RedirectToAction("Login", "User");
            }

            List<CartItem> cart = GetCartFromSession();
            CartItem? existingItem = cart.FirstOrDefault(item => item.FoodItemId == id);
            string itemName = "Item";

            if (existingItem != null)
            {
                itemName = existingItem.Name;
                cart.Remove(existingItem);
                TempData["SuccessMessage"] = $"{itemName} has been completely removed from your cart.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not find item in cart to remove.";
            }

            SaveCartToSession(cart);
            return RedirectToAction("Cart");
        }

        // --- ORDER & PAYMENT ACTIONS ---

        // NEW: Action to display the payment selection page
        public IActionResult Checkout()
        {
            string? userId = GetUserId();
            List<CartItem> cartItems = GetCartFromSession();

            if (string.IsNullOrEmpty(userId) || cartItems.Count == 0)
            {
                TempData["ErrorMessage"] = string.IsNullOrEmpty(userId) ? "Please log in to proceed." : "Your cart is empty.";
                return RedirectToAction("Menu", "Home");
            }

            // Calculate total and pass it to the view
            decimal cartTotal = cartItems.Sum(i => i.Price * i.Quantity);
            ViewData["CartTotal"] = cartTotal;

            // This should return the 'PaymentSelection.cshtml' view
            return View("PaymentSelection");
        }

        // NEW: Action to place the order and save it to the database
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string PaymentMethod)
        {
            // This now successfully calls the new GetUserId() helper
            string? userId = GetUserId();
            List<CartItem> cartItems = GetCartFromSession();

            if (string.IsNullOrEmpty(userId) || cartItems.Count == 0)
            {
                TempData["ErrorMessage"] = string.IsNullOrEmpty(userId) ? "Please log in to proceed." : "Your cart is empty.";
                return RedirectToAction("Menu", "Home");
            }

            decimal cartTotal = cartItems.Sum(i => i.Price * i.Quantity);

            // 2. Create Order Header 
            var order = new Orders
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cartTotal,
                PaymentMethod = PaymentMethod,
                OrderStatus = "Placed"
            };

            _context.orders.Add(order);

            // 4. Save to Database to get the Order.Id (Identity is populated here)
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Handle database errors if the Order table is not configured correctly
                TempData["ErrorMessage"] = "Error saving order details to the database. Ensure Order and OrderDetail tables exist.";
                return RedirectToAction("Cart");
            }

            // 3. Create Order Details
            foreach (var item in cartItems)
            {
                var orderDetail = new OrdersDetail
                {
                    OrderId = order.Id, // Use the generated ID
                    FoodItemId = item.FoodItemId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };
                _context.orderDetails.Add(orderDetail);
            }

            // 5. Save Order Details
            await _context.SaveChangesAsync();

            // 6. Clear Cart Session & Redirect
            HttpContext.Session.Remove("ShoppingCart");

            TempData["PaymentMethod"] = PaymentMethod;
            TempData["OrderId"] = order.Id;
            TempData["OrderTotal"] = cartTotal;

            bool showOnlineAlert = (PaymentMethod == "UPI");
            return RedirectToAction("OrderConfirmation", new { showOnlineAlert = showOnlineAlert });
        }

        // NEW: Action to display the order confirmation page
        public IActionResult OrderConfirmation(bool showOnlineAlert = false)
        {
            // Pass the flag to the view for the script logic (alert)
            ViewBag.ShowOnlineAlert = showOnlineAlert;

            // Check if essential data is in TempData (meaning a successful order just happened)
            if (TempData["OrderId"] == null)
            {
                // If the user tries to navigate here directly, redirect them away
                return RedirectToAction("Index", "Home");
            }

            // The view will retrieve the order details from TempData
            return View();
        }
    }
}