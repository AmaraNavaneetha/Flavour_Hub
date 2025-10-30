using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using restapp.Dal;
using restapp.Models;
using restapp.Services;

namespace restapp.Controllers
{
    public class FoodItemsController : Controller
    {
        private readonly RestContext _context;
        private readonly DBServices _dbs; // 1. ADDED: Injected DBServices

        // 1. UPDATED CONSTRUCTOR: Inject both Context and DBServices
        public FoodItemsController(RestContext context, DBServices dbs)
        {
            _context = context;
            _dbs = dbs;
        }

        // ADDED: Helper method for security check
        private bool IsAdmin()
        {
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            return loggedInUser != null && loggedinuserRole == "Admin";
        }

        // GET: FoodItems
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber, int? CategoryId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            ViewBag.loggedInUserId = HttpContext.Session.GetString("loggedinuser");
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["sellingpriceSortParm"] = sortOrder == "sellingprice" ? "sellingprice_desc" : "sellingprice";
            ViewData["ratingSortParm"] = sortOrder == "rating" ? "rating_desc" : "rating";

            // 1. DI Improvement: Use injected _dbs
            ViewBag.CategoryId = _dbs.GetCategorySelectItems();

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;
            IQueryable<FoodItem> fitems;

            if (CategoryId != null)
            {
                fitems = (from s in _context.fooditems.Include("category").Include("itemType")
                          where s.CategoryId == CategoryId
                          select s);
            }
            else
            {
                fitems = (from s in _context.fooditems.Include("category").Include("itemType")
                          select s);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                fitems = fitems.Where(s => s.ItemName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    fitems = fitems.OrderByDescending(s => s.ItemName);
                    break;
                case "sellingprice":
                    fitems = fitems.OrderBy(s => s.SellingPrice);
                    break;
                case "sellingprice_desc":
                    fitems = fitems.OrderByDescending(s => s.SellingPrice);
                    break;
                case "rating":
                    fitems = fitems.OrderBy(s => s.Rating);
                    break;
                case "rating_desc":
                    fitems = fitems.OrderByDescending(s => s.Rating);
                    break;
                default:
                    fitems = fitems.OrderBy(s => s.ItemName);
                    break;
            }

            int pageSize = 3;
            return View(await PaginatedList<FoodItem>.CreateAsync(fitems.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: FoodItems/Details/5 (Admin View)
        [HttpGet]
        public async Task<IActionResult> Details(int Id) // Made async
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            // Use FindAsync for async operation
            FoodItem f = await _context.fooditems.FindAsync(Id);

            if (f == null) return NotFound();
            return View(f);
        }

        // GET: FoodItems/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            ViewBag.loggedInUserId = HttpContext.Session.GetString("loggedinuser");

            // 1. DI Improvement: Use injected _dbs
            ViewBag.CategoryId = _dbs.GetCategorySelectItems();
            ViewBag.ItemTypeId = _dbs.GetItemTypeSelectListItem();

            return View();
        }

        // POST: FoodItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodItem f) // Made async
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            // Added Image check
            if (f.ItemImage == null)
            {
                ModelState.AddModelError("ItemImage", "Please upload an image.");
            }

            // Recalculate selling price before validation (assuming this is done here)
            f.SellingPrice = f.ActualPrice - (int)(f.ActualPrice * (f.DiscountPer / 100.0));

            if (ModelState.IsValid)
            {
                try
                {
                    // ⭐ IMPROVED: Safer, async file handling
                    var fileName = Path.GetFileName(f.ItemImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/FoodItems", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await f.ItemImage.CopyToAsync(stream);
                    }

                    f.ItemImagePath = $@"/images/FoodItems/{fileName}";

                    _context.fooditems.Add(f);
                    await _context.SaveChangesAsync(); // Use async save
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while saving the image or data.");
                }
            }

            // On failure: Use injected _dbs
            ViewBag.CategoryId = _dbs.GetCategorySelectItems();
            ViewBag.ItemTypeId = _dbs.GetItemTypeSelectListItem();
            return View(f);
        }

        // GET: FoodItems/Edit/5
        [HttpGet]
        public IActionResult Edit(int Id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            FoodItem f = _context.fooditems.Find(Id);

            // 1. DI Improvement: Use injected _dbs
            ViewBag.CategoryId = _dbs.GetCategorySelectItems();
            ViewBag.ItemTypeId = _dbs.GetItemTypeSelectListItem();

            return View(f);
        }

        // POST: FoodItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FoodItem fiC) // Made async
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            FoodItem fS = await _context.fooditems.FindAsync(fiC.ItemId); // Use FindAsync

            if (fS == null) return NotFound();

            // ⭐ IMPROVED: Safer, async file handling
            if (fiC.ItemImage != null)
            {
                try
                {
                    var fileName = Path.GetFileName(fiC.ItemImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/FoodItems", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fiC.ItemImage.CopyToAsync(stream);
                    }
                    fS.ItemImagePath = $@"/images/FoodItems/{fileName}";
                }
                catch (Exception)
                {
                    ModelState.AddModelError("ItemImage", "Error saving new image file.");
                }
            }

            // Update properties on the tracked entity (fS)
            fS.ItemName = fiC.ItemName;
            fS.ItemDescription = fiC.ItemDescription;
            fS.ActualPrice = fiC.ActualPrice;
            fS.DiscountPer = fiC.DiscountPer;
            // Recalculate selling price 
            fS.SellingPrice = fiC.ActualPrice - (int)(fiC.ActualPrice * (fiC.DiscountPer / 100.0));
            fS.Rating = fiC.Rating;
            fS.RatingCount = fiC.RatingCount;
            fS.IsAvailable = fiC.IsAvailable;
            fS.IsBestSeller = fiC.IsBestSeller;
            fS.IsFastMoving = fiC.IsFastMoving;
            fS.IsBreakfast = fiC.IsBreakfast;
            fS.IsLunch = fiC.IsLunch;
            fS.IsDinner = fiC.IsDinner;
            fS.CategoryId = fiC.CategoryId;
            fS.ItemTypeId = fiC.ItemTypeId;

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync(); // Use async save
                return RedirectToAction(nameof(Index));
            }

            // On failure: Use injected _dbs
            ViewBag.CategoryId = _dbs.GetCategorySelectItems();
            ViewBag.ItemTypeId = _dbs.GetItemTypeSelectListItem();
            return View(fiC);
        }

        // GET: FoodItems/Delete/5 (Confirmation page)
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            if (id == null) return NotFound();

            var foodItem = await _context.fooditems
                .Include(f => f.category)
                .Include(f => f.itemType)
                .FirstOrDefaultAsync(m => m.ItemId == id);

            if (foodItem == null) return NotFound();

            return View(foodItem);
        }

        // POST: FoodItems/Delete/5 (Execution)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            var foodItem = await _context.fooditems.FindAsync(id);
            if (foodItem != null)
            {
                _context.fooditems.Remove(foodItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FoodItemExists(int id)
        {
            return _context.fooditems.Any(e => e.ItemId == id);
        }

        // =========================================================================
        // === PUBLIC-FACING ACTIONS ===============================================
        // =========================================================================

        // ⭐ CRITICAL SECURITY FIX APPLIED HERE
        public IActionResult ByCategory(int categoryId)
        {
            // 1. SECURITY CHECK: Ensure the category is active
            var category = _context.categories.Find(categoryId);

            if (category == null || !category.CategoryStatus)
            {
                TempData["AlertMessage"] = "Sorry, that food category is currently unavailable.";
                return RedirectToAction("Menu", "Home");
            }

            // 2. Data Fetch: Use the injected _dbs service (which filters by IsAvailable)
            List<FoodItem> foodItems = _dbs.GetFoodItemsByCategory(categoryId);

            ViewData["CategoryName"] = category.CategoryName;
            ViewData["CategoryDiscount"] = category.CategoryDiscount;

            return View(foodItems);
        }

        // =========================================================================
        // === TOGGLE ACTIONS FOR FOOD ITEM MANAGEMENT (Admin) =======================
        // =========================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleItemAvailable(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            var item = await _context.fooditems.FindAsync(id);
            if (item == null) return NotFound();

            item.IsAvailable = !item.IsAvailable;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBestSeller(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            var item = await _context.fooditems.FindAsync(id);
            if (item == null) return NotFound();

            item.IsBestSeller = !item.IsBestSeller;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFastMoving(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            var item = await _context.fooditems.FindAsync(id);
            if (item == null) return NotFound();

            item.IsFastMoving = !item.IsFastMoving;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTag(int id, string tag)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "User");

            var item = await _context.fooditems.FindAsync(id);
            if (item == null) return NotFound();

            switch (tag.ToLower())
            {
                case "breakfast":
                    item.IsBreakfast = !item.IsBreakfast;
                    break;
                case "lunch":
                    item.IsLunch = !item.IsLunch;
                    break;
                case "dinner":
                    item.IsDinner = !item.IsDinner;
                    break;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}