using Laptop88_3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Laptop88_3.Filters;
namespace Laptop88_3.Controllers
{
    [AdminAuthorize]
    public class GiftProductManagermentController : Controller
    {
        
        private readonly AppDbContext db;
        public GiftProductManagermentController()
        {
            db = new AppDbContext();
        }

        public ActionResult Index()
        {
            
            var gifts = db.ProductGifts
                .Include(pg => pg.GiftProduct)
                .Include(pg => pg.MainProduct)
                .Include(pg => pg.Promotion)
                .ToList();

            
            var model = gifts.Select(g => new GiftProductViewModel
            {
                ProductGiftID = g.ProductGiftID,
                MainProductID = g.MainProductID,
                MainProductName = g.MainProduct?.ProductName ?? "(Không có)",
                GiftProductID = g.GiftProductID,
                GiftProductName = g.GiftProduct?.ProductName ?? "(Không có)",
                Quantity = g.Quantity,
                PromotionID = g.PromotionID,
                PromotionName = g.Promotion?.PromotionName ?? "Không có"
            }).ToList();

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }

        // Nested ViewModel cho quà tặng
        public class GiftProductViewModel
        {
            public int ProductGiftID { get; set; }

            public string MainProductID { get; set; }
            public string MainProductName { get; set; }

            public string GiftProductID { get; set; }
            public string GiftProductName { get; set; }

            public int Quantity { get; set; }

            public int? PromotionID { get; set; }
            public string PromotionName { get; set; }
        }
    }
}