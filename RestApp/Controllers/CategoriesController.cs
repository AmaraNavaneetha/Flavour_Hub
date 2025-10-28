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
            //get values from session
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");
            if (loggedInUser != null && loggedinuserRole == "Admin")
            {
                ViewBag.LoggedInUserId = loggedInUser;


                //List<Category> SliderList = _context.categories.ToList();
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
                return View(await PaginatedList<Category>.CreateAsync(categories.AsNoTracking(), pageNumber ?? 1, pageSize));
            }
            else
            {
                return RedirectToAction("Login", "User");//Login.cshtml + _Layout.cshtml
            }
        }

        // GET: Category/Details/5
        [HttpGet]
        public IActionResult Details(int Id)
        {
            //get values from session
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser != null && loggedinuserRole == "Admin")
            {
                Category c = _context.categories.Find(Id);
                return View(c); // returns slider's   Details.cshtml + _LayoutAdmin.cshtml
            }
            else
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
        }
        [HttpGet]
        public IActionResult Create()// to return empty view to add new slider data including image data
        {
            //get values from session
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser != null && loggedinuserRole == "Admin")
            {
                ViewBag.loggedInUserId = loggedInUser;
                return View(); // returns slider's   Create.cshtml + _LayoutAdmin.cshtml
            }
            else
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Slider/Create
        [HttpPost]
        public IActionResult Create(Category c)
        {
            //write validation logic here
            //saving file at server side file system
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories", c.CategoryImage.FileName);
            FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            c.CategoryImage.CopyTo(stream);

            //slider information with file info in db
            c.CategoryImagePath = @"/images/categories/" + c.CategoryImage.FileName;
            if (ModelState.IsValid)
            {
                _context.categories.Add(c);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(c); // create.cshtml with object s 
            }
        }

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
                Category c = _context.categories.Find(Id);
                return View(c); // returns slider's Edit.cshtml + _LayoutAdmin.cshtml
            }
            else
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]

        public IActionResult Edit(Category upC)
        {
            //responds with post request
            //ups - updated slider 
            // es- existing slider finding ,to modify that slider

            Category cS = _context.categories.Find(upC.CategoryId);
            var filePath = "";

            //write server side validation logic here if required
            //saving file at server side file system
            //if new slider image is available
            //from client side , do we have received new image or not

            if (upC.CategoryImage != null)
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories", upC.CategoryImage.FileName);
                FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                upC.CategoryImage.CopyTo(stream);
                //replace old path with new path
                cS.CategoryImagePath = @"/images/categories/" + upC.CategoryImage.FileName;
            }
            cS.CategoryName = upC.CategoryName;
            cS.CategoryDescription = upC.CategoryDescription;
            cS.CategoryStatus = upC.CategoryStatus;
            cS.CategoryDiscount = upC.CategoryDiscount;

            if (ModelState.IsValid)
            {
                //update database
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(upC); // create.cshtml with object s 
            }
        }
        // GET: Categories/Delete/5
        public IActionResult Delete(int Id)
        {
            Category c = _context.categories.Find(Id);
            _context.categories.Remove(c);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
