using Microsoft.AspNetCore.Mvc;

namespace restapp.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index() // returns Index.cshtml + _LayoutAdmin.cshtml
        {
            //get values from session
            string loggedInUser = HttpContext.Session.GetString("loggedinuser");
            string loggedinuserRole = HttpContext.Session.GetString("loggedinuserRole");

            if(loggedInUser != null && loggedinuserRole =="Admin")
            {
                ViewBag.loggedInUserId  = loggedInUser;
                return View(); // returns Index.cshtml + _LayoutAdmin.cshtml
            }
            else 
            {
                return RedirectToAction("Login", "User"); // Login.cshtml + _Layout.cshtml
            }
 
        }
    }
}
