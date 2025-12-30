using Laptop88_3.Filters;
using Laptop88_3.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Laptop88_3.Controllers
{
    
    public class UserController : Controller
    {
        private AppDbContext db = new AppDbContext();

        
        [HttpGet]
        public ActionResult Profile()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null) return HttpNotFound();

            return View(user); 
        }

        
        [HttpGet]
        public ActionResult InfoPartial()
        {
            if (Session["UserID"] == null)
                return new HttpStatusCodeResult(401); 

            int userId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null) return HttpNotFound();

            return PartialView("_Info", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateInfo(User model)
        {
            if (Session["UserID"] == null)
                return new HttpStatusCodeResult(401);

            int loggedInUserId = Convert.ToInt32(Session["UserID"]);
            if (model.UserID != loggedInUserId)
                return new HttpStatusCodeResult(403); 

            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.UserID == model.UserID);
                if (user == null) return HttpNotFound();

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;

                db.SaveChanges();

                return PartialView("_Info", user);
            }

            return PartialView("_Info", model);
        }
        [HttpGet]
        public ActionResult CartPartial()
        {
            int? userId = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : (int?)null;
            string userKey = Request.Cookies["UserKey"]?.Value;

            var cartItems = db.CartItems
                .Where(c => (userId != null && c.UserID == userId) ||
                            (userId == null && c.UserKey == userKey))
                .Select(c => new CartViewModel
                {
                    CartItemID = c.CartItemID,
                    ProductID = c.ProductID,
                    ProductName = c.Product.ProductName,
                    ImageURL = c.Product.ImageURL,
                    Price = c.Product.Price,
                    Quantity = c.Quantity,
                    DiscountPercent = c.Product.ProductPromotions
                        .Where(pp => pp.Promotion.IsActive &&
                                     pp.Promotion.StartDate <= DateTime.Now &&
                                     pp.Promotion.EndDate >= DateTime.Now)
                        .OrderByDescending(pp => pp.Promotion.DiscountPercent)
                        .Select(pp => (decimal?)pp.Promotion.DiscountPercent)
                        .FirstOrDefault()
                }).ToList();

            foreach (var item in cartItems)
            {
                item.FinalPrice = item.DiscountPercent.HasValue
                    ? item.Price * (100 - item.DiscountPercent.Value) / 100
                    : item.Price;
            }

            ViewBag.TongTien = cartItems.Sum(i => i.FinalPrice * i.Quantity);

            return PartialView("_Cart", cartItems);
        }


    }
}
