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
    public class PromotionManagementController : Controller
    {
        private readonly AppDbContext db;

        public PromotionManagementController()
        {
            db = new AppDbContext();
        }

        
        public ActionResult Index()
        {
            var promotions = db.Promotions
                .Include(p => p.ProductPromotions.Select(pp => pp.Product))
                .ToList();

            var model = promotions.Select(promo =>
            {
                
                var gifts = db.ProductGifts
                    .Where(pg => pg.PromotionID == promo.PromotionID)
                    .Select(pg => new
                    {
                        GiftName = pg.GiftProduct.ProductName,
                        Quantity = pg.Quantity,
                        MainProductName = pg.MainProduct.ProductName
                    })
                    .ToList();

                
                var products = promo.ProductPromotions
                    .Select(pp => pp.Product.ProductName)
                    .ToList();

                return new PromotionViewModel
                {
                    PromotionID = promo.PromotionID,
                    PromotionName = promo.PromotionName,
                    Description = promo.Description,
                    DiscountPercent = promo.DiscountPercent,
                    PricePromotion = promo.PricePromotion,
                    StartDate = promo.StartDate,
                    EndDate = promo.EndDate,
                    DisplayType = promo.DisplayType,
                    IsActive = promo.IsActive,
                    GiftList = gifts.Select(g => $"{g.GiftName} (x{g.Quantity})").ToList(),
                    ProductList = products 
                };
            }).ToList();

            return View(model);
        }
        [HttpGet]
        public ActionResult Create()
        {
            var model = new PromotionCreateViewModel
            {
                Products = db.Products.Select(x => new SelectListItem
                {
                    Value = x.ProductID.ToString(),
                    Text = x.ProductName
                }).ToList(),

                Gifts = db.Products.Select(x => new SelectListItem
                {
                    Value = x.ProductID.ToString(),
                    Text = x.ProductName
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PromotionCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Products = db.Products.Select(x => new SelectListItem
                {
                    Value = x.ProductID.ToString(),
                    Text = x.ProductName
                }).ToList();

                model.Gifts = db.Products.Select(x => new SelectListItem
                {
                    Value = x.ProductID.ToString(),
                    Text = x.ProductName
                }).ToList();

                return View(model);
            }

            var promotion = new Promotion
            {
                PromotionName = model.PromotionName,
                Description = model.Description,
                DiscountPercent = model.DiscountPercent,
                PricePromotion = model.PricePromotion,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                DisplayType = model.DisplayType,
                IsActive = model.IsActive
            };

            db.Promotions.Add(promotion);
            db.SaveChanges();

           
            if (model.SelectedProductIDs != null)
            {
                foreach (var productId in model.SelectedProductIDs)
                {
                    db.ProductPromotions.Add(new ProductPromotion
                    {
                        PromotionID = promotion.PromotionID,
                        ProductID = productId
                    });
                }
            }

            
            if (model.SelectedGiftProductIDs != null)
            {
                foreach (var giftId in model.SelectedGiftProductIDs)
                {
                    db.ProductGifts.Add(new ProductGift
                    {
                        PromotionID = promotion.PromotionID,
                        GiftProductID = giftId,
                        Quantity = 1
                    });
                }
            }

            db.SaveChanges();

            TempData["SuccessAdd"] = true;
            return RedirectToAction("Create");
        }
        

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var promo = db.Promotions
                          .Include(p => p.ProductPromotions)
                          .FirstOrDefault(p => p.PromotionID == id);

            if (promo == null) return HttpNotFound();

            
            var selectedProductIds = promo.ProductPromotions
                                          .Select(pp => pp.ProductID)
                                          .ToList();

            var selectedGiftIds = db.ProductGifts
                                     .Where(pg => pg.PromotionID == id)
                                     .Select(pg => pg.GiftProductID)
                                     .ToList();

            var model = new PromotionCreateViewModel
            {
                PromotionID = promo.PromotionID,
                PromotionName = promo.PromotionName,
                Description = promo.Description,
                DiscountPercent = promo.DiscountPercent,
                PricePromotion = promo.PricePromotion,
                StartDate = promo.StartDate,
                EndDate = promo.EndDate,
                DisplayType = promo.DisplayType,
                IsActive = promo.IsActive,

           
                Products = db.Products.Select(x => new SelectListItem
                {
                    Value = x.ProductID,      // string
                    Text = x.ProductName,
                    Selected = selectedProductIds.Contains(x.ProductID) // So sánh string ok
                }).ToList(),

              
                Gifts = db.Products.Select(x => new SelectListItem
                {
                    Value = x.ProductID,      // string
                    Text = x.ProductName,
                    Selected = selectedGiftIds.Contains(x.ProductID)
                }).ToList(),

                SelectedProductIDs = selectedProductIds,
                SelectedGiftProductIDs = selectedGiftIds
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, PromotionCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // reload danh sách products và gifts
                model.Products = db.Products.Select(x => new SelectListItem
                {
                    Value = x.ProductID.ToString(),
                    Text = x.ProductName
                }).ToList();

                model.Gifts = db.Products.Select(x => new SelectListItem
                {
                    Value = x.ProductID.ToString(),
                    Text = x.ProductName
                }).ToList();

                return View(model);
            }

            var promo = db.Promotions
                          .Include(p => p.ProductPromotions)
                          .FirstOrDefault(p => p.PromotionID == id);
            if (promo == null) return HttpNotFound();

            
            promo.PromotionName = model.PromotionName;
            promo.Description = model.Description;
            promo.DiscountPercent = model.DiscountPercent;
            promo.PricePromotion = model.PricePromotion;
            promo.StartDate = model.StartDate;
            promo.EndDate = model.EndDate;
            promo.DisplayType = model.DisplayType;
            promo.IsActive = model.IsActive;

            
            db.ProductPromotions.RemoveRange(promo.ProductPromotions);
            if (model.SelectedProductIDs != null)
            {
                foreach (var productIdStr in model.SelectedProductIDs)
                {
                    db.ProductPromotions.Add(new ProductPromotion
                    {
                        PromotionID = promo.PromotionID,
                        ProductID = productIdStr 
                    });
                }
            }

            var oldGifts = db.ProductGifts.Where(pg => pg.PromotionID == promo.PromotionID).ToList();
            db.ProductGifts.RemoveRange(oldGifts);

      
            if (model.SelectedProductIDs != null && model.SelectedGiftProductIDs != null)
            {
                foreach (var mainProductId in model.SelectedProductIDs)
                {
                    foreach (var giftIdStr in model.SelectedGiftProductIDs)
                    {
                        db.ProductGifts.Add(new ProductGift
                        {
                            PromotionID = promo.PromotionID,
                            MainProductID = mainProductId,  
                            GiftProductID = giftIdStr,
                            Quantity = 1
                        });
                    }
                }
            }


            db.SaveChanges();
            TempData["SuccessEdit"] = true;
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            try
            {
                var promo = db.Promotions
                              .Include(p => p.ProductPromotions)
                              .FirstOrDefault(p => p.PromotionID == id);

                if (promo == null)
                {
                    TempData["error"] = "Không tìm thấy khuyến mại cần xóa!";
                    return RedirectToAction("Index");
                }

                
                db.ProductPromotions.RemoveRange(promo.ProductPromotions);

                
                var giftList = db.ProductGifts.Where(pg => pg.PromotionID == id).ToList();
                db.ProductGifts.RemoveRange(giftList);

                db.Promotions.Remove(promo);

                db.SaveChanges();

                
            }
            catch (Exception)
            {
                TempData["error"] = "Có lỗi xảy ra khi xóa khuyến mại!";
            }

            return RedirectToAction("Index");
        }
    }

    public class PromotionViewModel
    {
        public int PromotionID { get; set; }
        public string PromotionName { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal PricePromotion { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int DisplayType { get; set; }
        public List<string> GiftList { get; set; } = new List<string>();
        public List<string> ProductList { get; set; } = new List<string>(); // ⬅ thêm mới
    }
    public class PromotionCreateViewModel
    {
        public int PromotionID { get; set; } // <-- thêm

        public string PromotionName { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal PricePromotion { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DisplayType { get; set; }
        public bool IsActive { get; set; }

        public List<SelectListItem> Products { get; set; }
        public List<string> SelectedProductIDs { get; set; } = new List<string>();

        public List<SelectListItem> Gifts { get; set; }
        public List<string> SelectedGiftProductIDs { get; set; } = new List<string>();
    }

}
