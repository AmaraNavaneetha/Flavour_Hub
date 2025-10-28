using Microsoft.AspNetCore.Mvc;
using restapp.Dal;
using restapp.Models;

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
    }
}
