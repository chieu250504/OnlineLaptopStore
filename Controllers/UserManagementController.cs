using Laptop88_3.Filters;
using Laptop88_3.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Laptop88_3.Controllers
{
    [AdminAuthorize]
    public class UserManagementController : Controller
    {
        private readonly AppDbContext db;

        public UserManagementController()
        {
            db = new AppDbContext();
        }

       
        public ActionResult Index()
        {
            var users = db.Users.ToList();   
            return View(users);
        }

      
        public ActionResult Details(int id)
        {
            var user = db.Users.FirstOrDefault(x => x.UserID == id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }
        [HttpGet]
        public ActionResult Create()
        {
            var model = new User();
            return View(model);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CreatedAt = DateTime.Now;

            db.Users.Add(model);
            db.SaveChanges();

            TempData["SuccessAdd"] = true;
            return RedirectToAction("Create");
        }
        
    }
}
