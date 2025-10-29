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

        public FoodItemsController(RestContext context)
        {
            _context = context;
        }

        // GET: FoodItems
        // GET: FoodItems
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber, int? CategoryId)
        {



            //get values from session
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");

            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser != null && loggedinuserRole == "Admin")

            {
                ViewBag.loggedInUserId = loggedInUser;
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewData["sellingpriceSortParm"] = sortOrder == "sellingprice" ? "sellingprice_desc" : "sellingprice";
                ViewData["ratingSortParm"] = sortOrder == "rating" ? "rating_desc" : "rating";

                List<SelectListItem> optionsList = new List<SelectListItem>();
                foreach (Category c in _context.categories)
                {
                    if (CategoryId == c.CategoryId)
                    {
                        optionsList.Add(new SelectListItem(c.CategoryName, c.CategoryId.ToString(), true));
                    }
                    else
                    {
                        optionsList.Add(new SelectListItem(c.CategoryName, c.CategoryId.ToString(), false));
                    }
                }
                ViewBag.CategoryId = optionsList;

                if (searchString != null)
                {
                    pageNumber = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewData["CurrentFilter"] = searchString;
                IQueryable<FoodItem>? fitems;
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
                //return View(await fitems.AsNoTracking().ToListAsync());

            }
            else
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
        }
        // GET: FoodItems/Details/5
        [HttpGet]
        public IActionResult Details(int Id)
        {
            //get values from session
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser != null && loggedinuserRole == "Admin")
            {
                FoodItem f = _context.fooditems.Find(Id);
                return View(f); // returns slider's   Details.cshtml + _LayoutAdmin.cshtml
            }
            else
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
        }
        // GET: FoodItems/Create
        [HttpGet]
        public IActionResult Create()// to return empty view to add new slider data including image data
        {
            //get values from session
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser != null && loggedinuserRole == "Admin")
            {
                ViewBag.loggedInUserId = loggedInUser;

                // category data
                ViewBag.CategoryId = new DBServices().GetCategorySelectItems();

                // item type data

                ViewBag.ItemTypeId = new DBServices().GetItemTypeSelectListItem();

                return View(); // returns slider's   Create.cshtml + _LayoutAdmin.cshtml
            }
            else
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
        }

        // POST: FoodItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public IActionResult Create(FoodItem f)
        {
            //write validation logic here
            //saving file at server side file system
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/FoodItems", f.ItemImage.FileName);
            FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            f.ItemImage.CopyTo(stream);

            //slider information with file info in db
            f.ItemImagePath = @"/images/FoodItems/" + f.ItemImage.FileName;
            if (ModelState.IsValid)
            {
                _context.fooditems.Add(f);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                // category data
                ViewBag.CategoryId = new DBServices().GetCategorySelectItems();

                // item type data

                ViewBag.ItemTypeId = new DBServices().GetItemTypeSelectListItem();
                return View(f); // create.cshtml with object f
            }
        }

        // GET: FoodItems/Edit/5
        [HttpGet]
        public IActionResult Edit(int Id)
        {
            //responds with get request
            //this is used to get/to get displayed ,the required data that is needed to be modified
            //get values from session
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser != null && loggedinuserRole == "Admin")
            {
                FoodItem f = _context.fooditems.Find(Id);
                return View(f); // returns slider's Edit.cshtml + _LayoutAdmin.cshtml
            }
            else
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
        }

        // POST: FoodItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]

        public IActionResult Edit(FoodItem fiC)
        {
            //responds with post request
            //ups - updated slider 
            // es- existing slider finding ,to modify that slider

            FoodItem fS = _context.fooditems.Find(fiC.ItemId);
            var filePath = "";

            //write server side validation logic here if required
            //saving file at server side file system
            //if new slider image is available
            //from client side , do we have received new image or not

            if (fiC.ItemImage != null)
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/FoodItems", fiC.ItemImage.FileName);
                FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                fiC.ItemImage.CopyTo(stream);
                //replace old path with new path
                fS.ItemImagePath = @"/images/FoodItems/" + fiC.ItemImage.FileName;
            }
            fS.ItemName = fiC.ItemName;
            fS.ItemDescription = fiC.ItemDescription;
            fS.ActualPrice = fiC.ActualPrice;
            fS.DiscountPer = fiC.DiscountPer;
            fS.SellingPrice = fiC.SellingPrice;
            fS.Rating = fiC.Rating;
            fS.RatingCount = fiC.RatingCount;
            fS.IsAvailable = fiC.IsAvailable;
            fS.IsBestSeller = fiC.IsBestSeller;
            fS.IsFastMoving = fiC.IsFastMoving;
            fS.IsBreakfast = fiC.IsBreakfast;
            fS.IsLunch = fiC.IsLunch;
            fS.IsDinner = fiC.IsDinner;


            if (ModelState.IsValid)
            {
                //update database
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                // category data
                ViewBag.CategoryId = new DBServices().GetCategorySelectItems();

                // item type data

                ViewBag.ItemTypeId = new DBServices().GetItemTypeSelectListItem();
                return View(fiC); // create.cshtml with object s 
            }
        }

        // GET: FoodItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItem
                .Include(f => f.category)
                .Include(f => f.itemType)
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        // POST: FoodItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foodItem = await _context.FoodItem.FindAsync(id);
            if (foodItem != null)
            {
                _context.FoodItem.Remove(foodItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FoodItemExists(int id)
        {
            return _context.FoodItem.Any(e => e.ItemId == id);
        }
        public IActionResult ByCategory(int categoryId)
        {
            var dbs = new DBServices();
            List<FoodItem> foodItems = dbs.GetFoodItemsByCategory(categoryId);
            return View(foodItems);
        }

    }

}

