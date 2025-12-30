
using System;
using System.Linq;
using System.Web.Mvc;
using Laptop88_3.Models;
using System.Data.Entity;
namespace Laptop88_3.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            var now = DateTime.Now;

            Func<Product, ProductWithSpecs> mapProduct = p =>
            {
                var activePromotion = p.ProductPromotions
                    .Where(pp => pp.Promotion.IsActive
                           && pp.Promotion.StartDate <= now
                           && pp.Promotion.EndDate >= now)
                    .Select(pp => pp.Promotion)
                    .ToList();

                decimal? discountPercent = null;
                decimal? progressPercent = null;
                decimal finalPrice = p.Price;

                if (activePromotion.Any())
                {
                    var bestPromotion = activePromotion
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
                    PromoEndDate = activePromotion.Any() ? activePromotion.First().EndDate : (DateTime?)null,
                    DisplayType = activePromotion.Any() ? activePromotion.First().DisplayType : (int?)null  // thêm dòng này
                };

            };

            ViewBag.TopProducts = db.Products
                .Include("LaptopSpecs")
                .Include("ProductPromotions.Promotion")
                .Where(p => p.LaptopSpecs != null)
                .Take(5)
                .ToList()
                .Select(mapProduct)
                .ToList();

            ViewBag.GamingProducts = db.Products
                .Include("LaptopSpecs")
                .Include("ProductPromotions.Promotion")
                .Where(p => p.LaptopSpecs != null &&
                            (p.LaptopSpecs.GraphicCard.ToLower().Contains("rtx") ||
                             p.LaptopSpecs.GraphicCard.ToLower().Contains("gtx")))
                .Take(5)
                .ToList()
                .Select(mapProduct)
                .ToList();

            ViewBag.StudentProducts = db.Products
                .Include("LaptopSpecs")
                .Include("ProductPromotions.Promotion")
                .Where(p => p.LaptopSpecs != null && p.Price < 17000000)
                .Take(5)
                .ToList()
                
                .ToList();

            ViewBag.HightProducts = db.Products
                .Include("LaptopSpecs")
                .Include("ProductPromotions.Promotion")
                .Where(p => p.LaptopSpecs != null && p.Price > 17000000)
                .Take(5)
                .ToList();

            ViewBag.MouseProducts = db.Products
                .Include("MouseSpecs")
                .Include("ProductPromotions.Promotion")
                .Where(p => p.MouseSpecs != null)
                .Take(5)
                .ToList();
            ViewBag.SaleProducts = db.Products
            .Include("LaptopSpecs")
            .Include("MouseSpecs")
            .Include("ProductPromotions.Promotion")
            .Where(p => p.ProductPromotions
                .Any(pp => pp.Promotion.DisplayType == 2
                           && pp.Promotion.IsActive
                           && DateTime.Now >= pp.Promotion.StartDate
                           && DateTime.Now <= pp.Promotion.EndDate))
            .ToList()              
            .Select(mapProduct)    
            .Take(5)
            .ToList();

            return View();
        }

        
    }
}
