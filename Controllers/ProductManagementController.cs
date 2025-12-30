using Laptop88_3.Filters;
using Laptop88_3.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Laptop88_3.Controllers
{
    [AdminAuthorize]
    public class ProductManagementController : Controller
    {
        private readonly AppDbContext db;

        public ProductManagementController()
        {
            db = new AppDbContext();
        }

        
        public ActionResult Index()
        {
            var products = db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductPromotions.Select(pp => pp.Promotion))
                .Include(p => p.ProductGifts.Select(pg => pg.GiftProduct))
                .ToList();

            var model = products.Select(p =>
            {
               
                var activePromotions = p.ProductPromotions
                    .Where(pp => pp.Promotion.IsActive &&
                                 pp.Promotion.StartDate <= DateTime.Now &&
                                 pp.Promotion.EndDate >= DateTime.Now)
                    .Select(pp => pp.Promotion)
                    .ToList();

                
                var pricesAfterPromo = new List<decimal> { p.Price };

                foreach (var promo in activePromotions)
                {
                    if (promo.PricePromotion > 0)
                        pricesAfterPromo.Add(promo.PricePromotion);
                    else if (promo.DiscountPercent > 0)
                        pricesAfterPromo.Add(p.Price * (1 - promo.DiscountPercent / 100));
                }

                var currentPrice = pricesAfterPromo.Min();

                return new ProductManagementViewModel
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    OriginalPrice = p.Price,
                    CurrentPrice = currentPrice,
                    PromotionsApplied = activePromotions.Count > 0
                            ? string.Join(", ", activePromotions.Select(x => x.PromotionName))
                            : "Không có",
                    GiftNames = p.ProductGifts?.Select(g => $"{g.GiftProduct.ProductName} (x{g.Quantity})").ToList()
                                ?? new List<string>(),
                    Stock = p.Stock,
                    ImageURL = p.ImageURL,
                    Color = p.color,
                    CategoryName = p.Category?.Name ?? "(Không có)",
                    CreatedAt = p.CreatedAt
                };
            }).ToList();

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }
        [HttpGet]
        public ActionResult Create()
        {
            var model = new ProductCreateEditViewModel
            {
                Categories = new SelectList(db.Categories, "CategoryID", "Name"),
                Colors = new SelectList(new[] { "Trắng", "Xám", "Bạc", "Đen", "Vàng", "Xanh", "Đỏ" }),

                Promotions = db.Promotions
                    .Where(p => p.IsActive)
                    .ToList()
                    .Select(p => new SelectListItem
                    {
                        Value = p.PromotionID.ToString(),
                        Text = $"{p.PromotionName} - {p.PricePromotion:N0}đ - {p.DiscountPercent}%"
                    }),

                Gifts = db.Products
                .ToList()   
                .Where(p => db.ProductGifts.Any(g => g.GiftProductID == p.ProductID))
                .Select(p => new SelectListItem
                {
                    Value = p.ProductID,
                    Text = $"{p.ProductName} - Giá: {p.Price:N0}đ"
                })

            };

            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = new SelectList(db.Categories, "CategoryID", "Name");
                model.Colors = new SelectList(new[] { "Trắng", "Xám", "Bạc", "Đen", "Vàng", "Xanh", "Đỏ" });
                return View(model);
            }

            var product = new Product
            {
                ProductID = model.ProductID,
                ProductName = model.ProductName,
                Price = model.Price,
                Stock = model.Stock,
                CategoryID = model.CategoryID,
                color = model.Color,
                ImageURL = model.ImageURL,
                CreatedAt = DateTime.Now
            };

            db.Products.Add(product);
            db.SaveChanges();

            
            if (model.SelectedPromotionIDs != null)
            {
                foreach (var promoID in model.SelectedPromotionIDs)
                {
                    db.ProductPromotions.Add(new ProductPromotion
                    {
                        ProductID = product.ProductID,
                        PromotionID = promoID
                    });
                }
            }

            
            if (model.SelectedGiftProductIDs != null)
            {
                foreach (var giftID in model.SelectedGiftProductIDs)
                {
                    db.ProductGifts.Add(new ProductGift
                    {
                        MainProductID = product.ProductID,
                        GiftProductID = giftID,
                        Quantity = 1
                    });
                }
            }

            db.SaveChanges();
            TempData["SuccessAdd"] = true;
            return RedirectToAction("Create");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["error"] = "Mã sản phẩm không hợp lệ.";
                return RedirectToAction("Index");
            }

            var product = db.Products
                .Include(p => p.ProductPromotions)
                .Include(p => p.ProductGifts)
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                TempData["error"] = "Không tìm thấy sản phẩm cần xóa.";
                return RedirectToAction("Index");
            }

            
            if (product.ProductPromotions != null && product.ProductPromotions.Any())
                db.ProductPromotions.RemoveRange(product.ProductPromotions);

            if (product.ProductGifts != null && product.ProductGifts.Any())
                db.ProductGifts.RemoveRange(product.ProductGifts);

            db.Products.Remove(product);
            db.SaveChanges();

            
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Edit(string id)
        {
            if (id == null) return HttpNotFound();

            var product = db.Products
                .Include(p => p.ProductPromotions)
                .Include(p => p.ProductGifts)
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null) return HttpNotFound();

            var model = new ProductCreateEditViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Price = product.Price,
                Stock = product.Stock,
                Color = product.color,
                CategoryID = product.CategoryID,
                ImageURL = product.ImageURL,

                
                SelectedPromotionIDs = product.ProductPromotions
                                        .Select(pp => pp.PromotionID)
                                        .ToList(),

                
                SelectedGiftProductIDs = product.ProductGifts
                                        .Select(g => g.GiftProductID)
                                        .ToList(),

                Categories = new SelectList(db.Categories, "CategoryID", "Name", product.CategoryID),
                Colors = new SelectList(new[] { "Trắng", "Xám", "Bạc", "Đen", "Vàng", "Xanh", "Đỏ" }, product.color),

                Promotions = db.Promotions
                    .Where(p => p.IsActive)
                    .ToList()
                    .Select(p => new SelectListItem
                    {
                        Value = p.PromotionID.ToString(),
                        Text = $"{p.PromotionName} - {p.PricePromotion:N0}đ - {p.DiscountPercent}%",
                        Selected = product.ProductPromotions.Any(pp => pp.PromotionID == p.PromotionID)
                    }),

                Gifts = db.Products
                    .ToList()
                    .Where(p => db.ProductGifts.Any(g => g.GiftProductID == p.ProductID))
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProductID,
                        Text = $"{p.ProductName} - Giá: {p.Price:N0}đ",
                        Selected = product.ProductGifts.Any(g => g.GiftProductID == p.ProductID)
                    })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = new SelectList(db.Categories, "CategoryID", "Name", model.CategoryID);
                model.Colors = new SelectList(new[] { "Trắng", "Xám", "Bạc", "Đen", "Vàng", "Xanh", "Đỏ" }, model.Color);
                return View(model);
            }

            var product = db.Products
                .Include(p => p.ProductPromotions)
                .Include(p => p.ProductGifts)
                .FirstOrDefault(p => p.ProductID == model.ProductID);

            if (product == null) return HttpNotFound();

            
            product.ProductName = model.ProductName;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.CategoryID = model.CategoryID;
            product.color = model.Color;
            product.ImageURL = model.ImageURL;

            db.Entry(product).State = EntityState.Modified;

            
            db.ProductPromotions.RemoveRange(product.ProductPromotions);

            if (model.SelectedPromotionIDs != null)
            {
                foreach (var promoID in model.SelectedPromotionIDs)
                {
                    db.ProductPromotions.Add(new ProductPromotion
                    {
                        ProductID = product.ProductID,
                        PromotionID = promoID
                    });
                }
            }

            
            db.ProductGifts.RemoveRange(product.ProductGifts);

            
            if (model.SelectedGiftProductIDs != null)
            {
                foreach (var giftID in model.SelectedGiftProductIDs)
                {
                    db.ProductGifts.Add(new ProductGift
                    {
                        MainProductID = product.ProductID,
                        GiftProductID = giftID,
                        Quantity = 1
                    });
                }
            }

            db.SaveChanges();

            TempData["SuccessEdit"] = true;
            return RedirectToAction("Index");
        }

    }

    public class ProductManagementViewModel
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public string PromotionsApplied { get; set; }     
        public List<string> GiftNames { get; set; }       
        public int Stock { get; set; }
        public string ImageURL { get; set; }
        public string Color { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class ProductCreateEditViewModel
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Color { get; set; }
        public int CategoryID { get; set; }
        public string ImageURL { get; set; }
        
        public List<int> SelectedPromotionIDs { get; set; } = new List<int>();

        
        public List<string> SelectedGiftProductIDs { get; set; } = new List<string>();

        public IEnumerable<SelectListItem> Promotions { get; set; }
        public IEnumerable<SelectListItem> Gifts { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> Colors { get; set; }
    }
}
