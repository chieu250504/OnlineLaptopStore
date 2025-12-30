using Laptop88_3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laptop88_3.Controllers
{
    public class CartController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Cart
        public ActionResult Index()
        {
            int? userId = null;
            string userKey = null;

            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (user != null)
                {
                    userId = user.UserID;
                }
            }
            else
            {
                if (Request.Cookies["UserKey"] != null)
                {
                    userKey = Request.Cookies["UserKey"].Value;
                }
            }

            // Lấy danh sách cart items theo UserID hoặc UserKey
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
                        .FirstOrDefault(),
                })
                .ToList();

            // Tính final price
            foreach (var item in cartItems)
            {
                if (item.DiscountPercent.HasValue)
                {
                    item.FinalPrice = item.Price * (100 - item.DiscountPercent.Value) / 100;
                }
                else
                {
                    item.FinalPrice = item.Price;
                }
            }

            ViewBag.TongTien = cartItems.Sum(i => i.FinalPrice * i.Quantity);

            return View(cartItems);
        }
        [HttpPost]
        public JsonResult Increase(int id)
        {
            var item = db.CartItems.FirstOrDefault(c => c.CartItemID == id);
            if (item != null)
            {
                item.Quantity++;
                db.SaveChanges();
            }

            var total = db.CartItems
                .Where(c => c.UserID == item.UserID || c.UserKey == item.UserKey)
                .Sum(c => c.Quantity * (c.Product.Price *
                    (1 - (c.Product.ProductPromotions
                            .Where(pp => pp.Promotion.IsActive &&
                                         pp.Promotion.StartDate <= DateTime.Now &&
                                         pp.Promotion.EndDate >= DateTime.Now)
                            .OrderByDescending(pp => pp.Promotion.DiscountPercent)
                            .Select(pp => (decimal?)pp.Promotion.DiscountPercent)
                            .FirstOrDefault() ?? 0) / 100)));

            return Json(new { qty = item.Quantity, total = total });
        }

        [HttpPost]
        public JsonResult Decrease(int id)
        {
            var item = db.CartItems.FirstOrDefault(c => c.CartItemID == id);
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                db.SaveChanges();
            }

            var total = db.CartItems
                .Where(c => c.UserID == item.UserID || c.UserKey == item.UserKey)
                .Sum(c => c.Quantity * (c.Product.Price *
                    (1 - (c.Product.ProductPromotions
                            .Where(pp => pp.Promotion.IsActive &&
                                         pp.Promotion.StartDate <= DateTime.Now &&
                                         pp.Promotion.EndDate >= DateTime.Now)
                            .OrderByDescending(pp => pp.Promotion.DiscountPercent)
                            .Select(pp => (decimal?)pp.Promotion.DiscountPercent)
                            .FirstOrDefault() ?? 0) / 100)));

            return Json(new { qty = item.Quantity, total = total });
        }

        [HttpPost]
        public JsonResult Remove(int id)
        {
            var item = db.CartItems.FirstOrDefault(c => c.CartItemID == id);
            if (item != null)
            {
                db.CartItems.Remove(item);
                db.SaveChanges();
            }

            var total = db.CartItems
                .Where(c => (c.UserID == item.UserID || c.UserKey == item.UserKey))
                .Sum(c => c.Quantity * (c.Product.Price *
                    (1 - (c.Product.ProductPromotions
                            .Where(pp => pp.Promotion.IsActive &&
                                         pp.Promotion.StartDate <= DateTime.Now &&
                                         pp.Promotion.EndDate >= DateTime.Now)
                            .OrderByDescending(pp => pp.Promotion.DiscountPercent)
                            .Select(pp => (decimal?)pp.Promotion.DiscountPercent)
                            .FirstOrDefault() ?? 0) / 100)));

            return Json(new { success = true, total = total });
        }

    }

    public class CartViewModel
    {
        public int CartItemID { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string ImageURL { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal? DiscountPercent { get; set; }
    }
}
