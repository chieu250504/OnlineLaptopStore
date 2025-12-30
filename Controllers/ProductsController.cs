using Laptop88_3.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity; 
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laptop88_3.Controllers
{
    public class ProductsController : Controller
    {
        private AppDbContext db = new AppDbContext();
        public ActionResult TestGiftProducts()
        {
            var query = from p in db.Products
                        join g in db.ProductGifts on p.ProductID equals g.MainProductID into pg
                        from g in pg.DefaultIfEmpty()
                        join gift in db.Products on g.GiftProductID equals gift.ProductID into gg
                        from gift in gg.DefaultIfEmpty()
                        join promo in db.Promotions on g.PromotionID equals promo.PromotionID into pp
                        from promo in pp.DefaultIfEmpty()
                        select new
                        {
                            MainProductID = p.ProductID,
                            MainProductName = p.ProductName,
                            GiftProductName = gift.ProductName,
                            PromotionName = promo.PromotionName,
                            DiscountPercent = promo.DiscountPercent,
                            StartDate = promo.StartDate,
                            EndDate = promo.EndDate
                        };

            
            foreach (var item in query.ToList())
            {
                System.Diagnostics.Debug.WriteLine(
                    $"{item.MainProductName} → Tặng: {item.GiftProductName} | KM: {item.PromotionName} ({item.DiscountPercent}%)");
            }

            
            return Json(query.ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Details(string id)
        {
            var now = DateTime.Now;

            
            var product = db.Products
                .Include(p => p.LaptopSpecs)
                .Include(p => p.MouseSpecs)
                .Include(p => p.ProductPromotions.Select(pp => pp.Promotion))
                .Include(p => p.ProductGifts.Select(pg => pg.GiftProduct))
                .Include(p => p.ProductGifts.Select(pg => pg.Promotion))
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            
            Func<Product, ProductWithSpecs> mapProduct = p =>
            {
                var activePromotions = p.ProductPromotions?
                    .Where(pp => pp.Promotion != null &&
                                 pp.Promotion.IsActive &&
                                 pp.Promotion.StartDate <= now &&
                                 pp.Promotion.EndDate >= now)
                    .Select(pp => pp.Promotion)
                    .ToList() ?? new List<Promotion>();

                decimal? discountPercent = null;
                decimal? progressPercent = null;
                decimal finalPrice = p.Price;

                if (activePromotions.Any())
                {
                    var bestPromotion = activePromotions
                        .OrderByDescending(pr => pr.DiscountPercent)
                        .First();

                    discountPercent = bestPromotion.DiscountPercent;

                    var totalDay = (bestPromotion.EndDate - bestPromotion.StartDate).TotalDays;
                    var remainingDay = (bestPromotion.EndDate - now).TotalDays;

                    if (totalDay > 0)
                    {
                        var rawPercent = (remainingDay / totalDay) * 100;
                        progressPercent = (decimal)Math.Max(0, Math.Min(100, rawPercent));
                    }

                    finalPrice = finalPrice * (100 - discountPercent.Value) / 100;
                }

                
                var gifts = p.ProductGifts?
                    .Where(g => g.GiftProduct != null)
                    .Select(g => new ProductWithSpecs
                    {
                        ProductID = g.GiftProduct.ProductID,
                        ProductName = g.GiftProduct.ProductName,
                        ImageURL = g.GiftProduct.ImageURL,
                        Price = g.GiftProduct.Price
                    }).ToList() ?? new List<ProductWithSpecs>();

                return new ProductWithSpecs
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    Price = p.Price,
                    ImageURL = p.ImageURL,
                    Brand = p.LaptopSpecs?.Brand,
                    CPU = p.LaptopSpecs?.CPU,
                    RAM = p.LaptopSpecs?.RAM,
                    GraphicCard = p.LaptopSpecs?.GraphicCard,
                    DPI = p.MouseSpecs?.DPI,
                    ConnectionType = p.MouseSpecs?.ConnectionType,
                    Battery = p.MouseSpecs?.Battery,
                    DiscountPercent = discountPercent,
                    FinalPrice = finalPrice,
                    PromoProgressPercent = progressPercent,
                    ProductGifts = gifts
                };
            };

            
            var mappedProduct = mapProduct(product);

            
            var prefix = id.Substring(0, 4);
            mappedProduct.RelatedProducts = db.Products
                .Include(p => p.LaptopSpecs)
                .Include(p => p.ProductPromotions.Select(pp => pp.Promotion))
                .Where(p => p.ProductID.StartsWith(prefix) && p.ProductID != id)
                .Take(9)
                .ToList()
                .Select(mapProduct)
                .ToList();

            return View(new List<ProductWithSpecs> { mappedProduct });
        }

        [HttpPost]
        public ActionResult AddToCart(string productId)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product == null) return HttpNotFound();

            int? userId = null;
            string userKey = null;

            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (user != null) userId = user.UserID;
            }
            else
            {
                if (Request.Cookies["UserKey"] != null)
                {
                    userKey = Request.Cookies["UserKey"].Value;
                }
                else
                {
                    userKey = Guid.NewGuid().ToString();
                    var cookie = new HttpCookie("UserKey", userKey);
                    cookie.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(cookie);
                }
            }

            var existingItem = db.CartItems.FirstOrDefault(c =>
                c.ProductID == productId &&
                ((userId != null && c.UserID == userId) ||
                 (userId == null && c.UserKey == userKey)));

            if (existingItem != null)
                existingItem.Quantity += 1;
            else
                db.CartItems.Add(new CartItem
                {
                    ProductID = productId,
                    Quantity = 1,
                    UserID = userId,
                    UserKey = userKey,
                    AddedAt = DateTime.Now
                });

            db.SaveChanges();
            return RedirectToAction("Index", "Cart");
        }
    }
}
