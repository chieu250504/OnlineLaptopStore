using System;
using System.Linq;
using System.Web.Mvc;
using Laptop88_3.Models;

namespace Laptop88_3.Controllers
{
    public class SearchApiController : Controller
    {
        private AppDbContext db = new AppDbContext();

        [HttpGet]
        public JsonResult Search(string query)
        {
            var now = DateTime.Now;

            var products = db.Products
                .Include("LaptopSpecs")
                .Include("MouseSpecs")
                .Include("ProductPromotions.Promotion")
                
                .Where(p => string.IsNullOrEmpty(query) || p.ProductName.Contains(query))
                .Take(20)
                .ToList()
                .Select(p =>
                {
                    
                    var activePromotion = p.ProductPromotions
                        .Where(pp => pp.Promotion.IsActive
                               && pp.Promotion.StartDate <= now
                               && pp.Promotion.EndDate >= now)
                        .Select(pp => pp.Promotion)
                        .ToList();

                    decimal? discountPercent = null;
                    decimal finalPrice = p.Price;

                    if (activePromotion.Any())
                    {
                        var bestPromotion = activePromotion
                            .OrderByDescending(pr => pr.DiscountPercent)
                            .First();

                        discountPercent = bestPromotion.DiscountPercent;
                        finalPrice = finalPrice * (100 - discountPercent.Value) / 100;
                    }

                    return new
                    {
                        p.ProductID,
                        p.ProductName,
                        p.ProductDescription,
                        p.Price,
                        p.ImageURL,

                        // LaptopSpecs
                        CPU = p.LaptopSpecs != null ? p.LaptopSpecs.CPU : null,
                        RAM = p.LaptopSpecs != null ? p.LaptopSpecs.RAM : null,
                        Storage = p.LaptopSpecs != null ? p.LaptopSpecs.Storage : null,
                        GraphicsCard = p.LaptopSpecs != null ? p.LaptopSpecs.GraphicCard : null,
                        Display = p.LaptopSpecs != null ? p.LaptopSpecs.Display : null,
                        // Mouspecs

                        DPI=p.MouseSpecs != null ? p.MouseSpecs.DPI : null,
                        ConnectionType=p.MouseSpecs!= null ? p.MouseSpecs.ConnectionType : null,
                        Brand=p.MouseSpecs!=null ? p.MouseSpecs.Brand : null,
                        Battery=p.MouseSpecs!=null? p.MouseSpecs.Battery : null,
                        DiscountPercent = discountPercent,
                        FinalPrice = finalPrice
                    };
                })
                .ToList();

            return Json(products, JsonRequestBehavior.AllowGet);
        }
    }
}
