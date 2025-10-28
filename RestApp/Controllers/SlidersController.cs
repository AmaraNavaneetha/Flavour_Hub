using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using restapp.Dal;
using restapp.Models;

namespace restapp.Controllers
{
    public class SlidersController : Controller
    {
        private readonly RestContext _context;

        public SlidersController(RestContext context)
        {
            _context = context;
        }

        // GET: Slider
        public IActionResult Index() // to make sure only admin access the sliders
        {
            //get values from session
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser != null && loggedinuserRole == "Admin")
            {
                ViewBag.loggedInUserId = loggedInUser;
                List<Sliders> sliderList = _context.sliders.ToList();
                return View(sliderList); // returns slider's   Index.cshtml + _LayoutAdmin.cshtml
            }
            else
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
            
        }

        // GET: Slider/Details/5
        [HttpGet]
        public IActionResult Details(int Id)
        {
            //get values from session
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if (loggedInUser != null && loggedinuserRole == "Admin")
            {
                Sliders s = _context.sliders.Find(Id);
                return View(s); // returns slider's   Details.cshtml + _LayoutAdmin.cshtml
            }
            else
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
        }

        // GET: Slider/Create
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
        
        // POST: Slider/Create
        [HttpPost]
        public IActionResult Create(Sliders s)
        {
            //write validation logic here
            //saving file at server side file system
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/sliders",s.SliderImage.FileName);
            FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            s.SliderImage.CopyTo(stream);

            //slider information with file info in db
            s.SliderImagePath = @"/images/sliders/" + s.SliderImage.FileName;
            if (ModelState.IsValid)
            {
                _context.sliders.Add(s);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(s); // create.cshtml with object s 
            }
        }

        // GET: Slider/Edit/5
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
                Sliders s = _context.sliders.Find(Id);
                return View(s); // returns slider's Edit.cshtml + _LayoutAdmin.cshtml
            }
            else
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
        }

        // POST: Slider/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        
        public IActionResult Edit(Sliders upS)
        {
            //responds with post request
            //ups - updated slider 
            // es- existing slider finding ,to modify that slider

            Sliders eS = _context.sliders.Find(upS.SliderId);
            var filePath = "";

            //write server side validation logic here if required
            //saving file at server side file system
            //if new slider image is available
            //from client side , do we have received new image or not

            if(upS.SliderImage != null)
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/sliders", upS.SliderImage.FileName);
                FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                upS.SliderImage.CopyTo(stream);
                //replace old path with new path
                eS.SliderImagePath = @"/images/sliders/" + upS.SliderImage.FileName;
            }
            eS.Name = upS.Name;
            eS.DisplayText = upS.DisplayText;
            eS.LinkText = upS.LinkText;
            eS.Status = upS.Status;
            eS.DisplayOrderNo = upS.DisplayOrderNo;

            if (ModelState.IsValid)
            {
                //update database
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(upS); // create.cshtml with object s 
            }
        }

        // GET: Slider/Delete/5
       
        public IActionResult Delete(int Id)
        {
            Sliders s = _context.sliders.Find(Id);
            _context.sliders.Remove(s);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool SliderExists(int id)
        {
            return _context.sliders.Any(e => e.SliderId == id);
        }
    }
}
