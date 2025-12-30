using Laptop88_3.Models;
using Laptop88_3.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace Laptop88_3.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly AppDbContext db;
        private readonly OpenProvinceService _openProvinceService;

        public CheckoutController()
        {
            db = new AppDbContext();
            _openProvinceService = new OpenProvinceService();
        }
        public ActionResult Index(string productId = null)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("Login", "Account");

            var query = db.CartItems.Where(c => c.UserID == user.UserID);
            if (!string.IsNullOrEmpty(productId))
                query = query.Where(c => c.ProductID == productId);

            var cartItemsList = query.ToList();

            var cartItems = cartItemsList.Select(c =>
            {
                var product = c.Product;
                decimal price = product?.Price ?? 0m;

                // 🔹 Tìm khuyến mãi còn hiệu lực
                var activePromotion = product?.ProductPromotions?
                    .Where(pp => pp.Promotion != null &&
                                 pp.Promotion.IsActive &&
                                 pp.Promotion.StartDate <= DateTime.Now &&
                                 pp.Promotion.EndDate >= DateTime.Now)
                    .OrderByDescending(pp => pp.Promotion.DiscountPercent)
                    .Select(pp => pp.Promotion)
                    .FirstOrDefault();

                decimal discountPercent = activePromotion?.DiscountPercent ?? 0m;
                decimal finalPrice = price * (100m - discountPercent) / 100m;

                // 🔹 Lấy quà tặng nếu có
                List<GiftProductViewModel> giftProducts = new List<GiftProductViewModel>();
                if (activePromotion != null)
                {
                    giftProducts = db.ProductGifts
                        .Where(g => g.MainProductID == product.ProductID &&
                                    g.PromotionID == activePromotion.PromotionID &&
                                    g.GiftProduct != null)
                        .Select(g => new GiftProductViewModel
                        {
                            GiftProductID = g.GiftProductID,
                            GiftProductName = g.GiftProduct.ProductName,
                            GiftImageURL = g.GiftProduct.ImageURL,
                            Quantity = g.Quantity
                        })
                        .ToList();
                }

                return new CheckoutItemViewModel
                {
                    ProductID = c.ProductID,
                    ProductName = product?.ProductName ?? "",
                    ImageURL = product?.ImageURL ?? "",
                    Price = price,
                    Quantity = c.Quantity,
                    DiscountPercent = discountPercent,
                    FinalPrice = finalPrice,
                    GiftProducts = giftProducts
                };
            }).ToList();

            var model = new CheckoutViewModel
            {
                Items = cartItems,
                TotalAmount = cartItems.Sum(i => i.FinalPrice * i.Quantity),
                Form = new CheckoutFormModel
                {
                    PhoneShipping = user.PhoneNumber,
                    PaymentMethod = "COD"
                }
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult PlaceOrder(CheckoutFormModel form)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return RedirectToAction("Index");

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("Login", "Account");

            var cartItems = db.CartItems
                .Include("Product.ProductPromotions.Promotion")
                .Where(c => c.UserID == user.UserID)
                .ToList();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            // 1️⃣ Tạo đơn hàng
            var order = new Order
            {
                UserID = user.UserID,
                OrderDate = DateTime.Now,
                Status = "Đang chờ duyệt",
                ShippingAddress = form.DetailAddress,
                ProvinceCode = form.ProvinceCode,
                DistrictCode = form.DistrictCode,
                WardCode = form.WardCode,
                PhoneShipping = form.PhoneShipping,
                DescriptionUser = form.UserDescription,
                TotalAmount = 0m
            };
            db.Orders.Add(order);
            db.SaveChanges(); // cần có OrderID

            // 2️⃣ Thêm chi tiết đơn hàng
            decimal total = 0m;
            foreach (var ci in cartItems)
            {
                var product = ci.Product;
                decimal price = product?.Price ?? 0m;

                var activePromotion = product?.ProductPromotions?
                    .Where(pp => pp.Promotion != null &&
                                 pp.Promotion.IsActive &&
                                 pp.Promotion.StartDate <= DateTime.Now &&
                                 pp.Promotion.EndDate >= DateTime.Now)
                    .Select(pp => pp.Promotion)
                    .FirstOrDefault();

                decimal discount = activePromotion?.DiscountPercent ?? 0m;
                decimal finalPrice = price * (100m - discount) / 100m;

                // 🔹 Thêm sản phẩm chính
                var orderDetail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = ci.ProductID,
                    Quantity = ci.Quantity,
                    UnitPrice = finalPrice
                };
                db.OrderDetails.Add(orderDetail);
                total += finalPrice * ci.Quantity;

                // 🔹 Thêm quà tặng nếu có
                if (activePromotion != null)
                {
                    var giftItems = db.ProductGifts
                        .Include("GiftProduct")
                        .Where(g => g.MainProductID == product.ProductID &&
                                    g.PromotionID == activePromotion.PromotionID &&
                                    g.GiftProduct != null)
                        .ToList();

                    foreach (var gift in giftItems)
                    {
                        var giftDetail = new OrderDetail
                        {
                            OrderID = order.OrderID,
                            ProductID = gift.GiftProductID,
                            Quantity = gift.Quantity,
                            UnitPrice = 0m
                        };
                        db.OrderDetails.Add(giftDetail);
                    }
                }
            }

            order.TotalAmount = total;
            db.SaveChanges();

            // 3️⃣ Tạo bản ghi Payment
            var payment = new Payment
            {
                OrderID = order.OrderID,
                PaymentMethod = form.PaymentMethod,
                PaidAmount = 0m,
                PaidAt = DateTime.Now,
                Status = "Unpaid"
            };
            db.Payments.Add(payment);
            db.SaveChanges();

            // 4️⃣ Xóa giỏ hàng
            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            // 5️⃣ Chuyển hướng
            if (form.PaymentMethod == "Online")
                return RedirectToAction("PaymentPage", "Payment", new { orderId = order.OrderID });

            return RedirectToAction("Success", new { orderId = order.OrderID });
        }

        public ActionResult Success(int orderId)
        {
            var order = db.Orders
                .Include("OrderDetails.Product")
                .Include("Payments")
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
                return RedirectToAction("Index", "Home");

            var payment = order.Payments.FirstOrDefault();
            string paymentMethod = payment?.PaymentMethod ?? "Không xác định";

            // 🔹 Sản phẩm chính
            var orderItems = order.OrderDetails
                .Where(d => d.UnitPrice > 0)
                .Select(d => new OrderSuccessItem
                {
                    ProductName = d.Product?.ProductName ?? "(Không xác định)",
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    ImageURL = d.Product?.ImageURL,
                    PaymentMethod = paymentMethod
                })
                .ToList();

            // 🔹 Quà tặng
            var giftItems = order.OrderDetails
                .Where(d => d.UnitPrice == 0 && d.Product != null)
                .Select(d => new OrderSuccessItem
                {
                    ProductName = d.Product?.ProductName ?? "(Quà tặng không xác định)",
                    Quantity = d.Quantity,
                    UnitPrice = 0m,
                    ImageURL = d.Product?.ImageURL,
                    PaymentMethod = paymentMethod
                })
                .ToList();

            var allItems = orderItems.Concat(giftItems).ToList();

            var model = new OrderSuccessViewModel
            {
                OrderID = order.OrderID,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                Status = order.Status,
                EstimatedDelivery = order.OrderDate.AddDays(3),
                PaymentMethod = paymentMethod,
                Items = allItems
            };

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }
    }

    // ================= ViewModels =================
    public class GiftProductViewModel
    {
        public string GiftProductID { get; set; }
        public string GiftProductName { get; set; }
        public string GiftImageURL { get; set; }
        public int Quantity { get; set; }
    }

    public class CheckoutItemViewModel
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string ImageURL { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public List<GiftProductViewModel> GiftProducts { get; set; } = new List<GiftProductViewModel>();
    }

    public class CheckoutViewModel
    {
        public List<CheckoutItemViewModel> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public CheckoutFormModel Form { get; set; }
    }

    public class CheckoutFormModel
    {
        [Required]
        public string ProvinceCode { get; set; }
        public string ProvinceName { get; set; }

        [Required]
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }

        [Required]
        public string WardCode { get; set; }
        public string WardName { get; set; }

        [Required]
        public string DetailAddress { get; set; }

        public string ShippingAddress => $"{DetailAddress}, {WardName}, {DistrictName}, {ProvinceName}";

        [Required]
        public string PhoneShipping { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
        public string UserDescription { get; set; }
    }

    public class OrderSuccessViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime EstimatedDelivery { get; set; }
        public List<OrderSuccessItem> Items { get; set; }
    }

    public class OrderSuccessItem
    {
        public string ProductName { get; set; }
        public string ImageURL { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string PaymentMethod { get; set; }
        public bool IsGift => UnitPrice == 0m;
    }
}
