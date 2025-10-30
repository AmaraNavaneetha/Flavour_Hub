using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using restapp.Dal;
using restapp.Models;

namespace restapp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly RestContext _context;

        public CategoriesController(RestContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            // Security Check
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser == null || loggedinuserRole != "Admin")
            {
                return RedirectToAction("Login", "User");
            }

            ViewBag.LoggedInUserId = loggedInUser;

            // Paging/Sorting/Filtering Logic (Remains as is)
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DiscountSortParm"] = sortOrder == "discount" ? "discount_desc" : "discount";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;

            var categories = from s in _context.categories
                             select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(s => s.CategoryName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    categories = categories.OrderByDescending(s => s.CategoryName);
                    break;
                case "discount":
                    categories = categories.OrderBy(s => s.CategoryDiscount);
                    break;
                case "discount_desc":
                    categories = categories.OrderByDescending(s => s.CategoryDiscount);
                    break;
                default:
                    categories = categories.OrderBy(s => s.CategoryName);
                    break;
            }

            int pageSize = 3;
            // Assuming PaginatedList is an async utility
            return View(await PaginatedList<Category>.CreateAsync(categories.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Category/Details/5 (Omitted for brevity, logic remains)
        // ...

        // GET: Category/Create (Omitted for brevity, logic remains)
        // ...

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category c) // Made async
        {
            // Security check (Can be moved to a filter/base controller)
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedInUser == null || loggedinuserRole != "Admin") return RedirectToAction("Login", "User");

            if (c.CategoryImage == null)
            {
                ModelState.AddModelError("CategoryImage", "Please select a category image.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ⭐ IMPROVED: Safer file handling using using statement
                    var fileName = Path.GetFileName(c.CategoryImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await c.CategoryImage.CopyToAsync(stream); // Use async copy
                    }

                    c.CategoryImagePath = $@"/images/categories/{fileName}";

                    _context.categories.Add(c);
                    await _context.SaveChangesAsync(); // Use async save
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the image or data.");
                    // Log the exception (ex) here
                }
            }
            return View(c);
        }

        // GET: Category/Edit/5 (Omitted for brevity, logic remains)
        // ...

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category upC) // Made async
        {
            // Security check (Can be moved to a filter/base controller)
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedInUser == null || loggedinuserRole != "Admin") return RedirectToAction("Login", "User");

            if (ModelState.IsValid)
            {
                var cS = await _context.categories.FindAsync(upC.CategoryId); // Use FindAsync

                if (cS == null) return NotFound();

                // ⭐ IMPROVED: Safer file handling
                if (upC.CategoryImage != null)
                {
                    try
                    {
                        var fileName = Path.GetFileName(upC.CategoryImage.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await upC.CategoryImage.CopyToAsync(stream);
                        }
                        cS.CategoryImagePath = $@"/images/categories/{fileName}";
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", "Error saving new image file.");
                        return View(upC);
                    }
                }

                // Update properties on the tracked entity (cS)
                cS.CategoryName = upC.CategoryName;
                cS.CategoryDescription = upC.CategoryDescription;
                cS.CategoryStatus = upC.CategoryStatus; // This is the key property!
                cS.CategoryDiscount = upC.CategoryDiscount;

                await _context.SaveChangesAsync(); // Use async save
                return RedirectToAction(nameof(Index));
            }
            return View(upC);
        }

        // GET: Categories/Delete/5 (Should just show confirmation)
        public async Task<IActionResult> Delete(int? id)
        {
            // Security check
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedInUser == null || loggedinuserRole != "Admin" || id == null)
            {
                return RedirectToAction("Login", "User");
            }

            var category = await _context.categories.FindAsync(id);
            if (category == null) return NotFound();

            // This GET action should display the Delete confirmation view
            return View(category);
        }

        // ⭐ NEW/CORRECTED: POST action for actual deletion
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Security check
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedInUser == null || loggedinuserRole != "Admin") return RedirectToAction("Login", "User");

            var category = await _context.categories.FindAsync(id);
            if (category != null)
            {
                _context.categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Categories/ToggleCategoryStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            // Security check
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser == null || loggedinuserRole != "Admin")
            {
                return RedirectToAction("Login", "User");
            }

            var category = await _context.categories.FindAsync(id);
            if (category == null) return NotFound();

            // Toggle the CategoryStatus property
            category.CategoryStatus = !category.CategoryStatus;
            await _context.SaveChangesAsync();

            // Redirect back to the list view
            return RedirectToAction(nameof(Index));
        }
    }
}