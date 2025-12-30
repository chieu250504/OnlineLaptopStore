using System;
using System.Linq;
using System.Web.Mvc;
using Laptop88_3.Models;
using System.Collections.Generic;
using System.Data.Entity;
using Laptop88_3.Filters;
namespace Laptop88_3.Controllers
{
    [AdminAuthorize]
    public class HomeAdminController : Controller
    {
        private readonly AppDbContext db;

        public HomeAdminController()
        {
            db = new AppDbContext();
        }

        
        public ActionResult Home()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            
            int ordersToday = db.Orders
                .Where(o => o.OrderDate != null && o.OrderDate >= today)
                .Count();

            int ordersYesterday = db.Orders
                .Where(o => o.OrderDate != null && o.OrderDate >= yesterday && o.OrderDate < today)
                .Count();

            decimal revenueToday = db.Orders
                .Where(o => o.OrderDate != null && o.TotalAmount != null && o.OrderDate >= today)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            decimal revenueYesterday = db.Orders
                .Where(o => o.OrderDate != null && o.TotalAmount != null && o.OrderDate >= yesterday && o.OrderDate < today)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            
            int newCustomersToday = db.Users
                .Where(u => u.CreatedAt != null && u.CreatedAt >= today)
                .Count();

            int newCustomersYesterday = db.Users
                .Where(u => u.CreatedAt != null && u.CreatedAt >= yesterday && u.CreatedAt < today)
                .Count();

           
            int pendingOrders = db.Orders
                .Where(o => !string.IsNullOrEmpty(o.Status) && o.Status == "Chờ xử lý")
                .Count();

            
            decimal ordersChange = ordersYesterday == 0 ? 0 : ((decimal)(ordersToday - ordersYesterday) / ordersYesterday) * 100;
            decimal revenueChange = revenueYesterday == 0 ? 0 : ((revenueToday - revenueYesterday) / revenueYesterday) * 100;
            decimal newCustomersChange = newCustomersYesterday == 0 ? 0 : ((decimal)(newCustomersToday - newCustomersYesterday) / newCustomersYesterday) * 100;

            
            var last10Days = Enumerable.Range(0, 10)
                                       .Select(i => today.AddDays(-i))
                                       .OrderBy(d => d)
                                       .ToList();

            List<string> chartLabels = new List<string>();
            List<decimal> chartData = new List<decimal>();

            foreach (var day in last10Days)
            {
                chartLabels.Add(day.ToString("dd/MM/yyyy"));

                
                var nextDay = day.AddDays(1);

                decimal dayRevenue = db.Orders
                    .Where(o => o.OrderDate != null
                                && o.TotalAmount != null
                                && o.OrderDate >= day
                                && o.OrderDate < nextDay
                                && !string.IsNullOrEmpty(o.Status)
                                && o.Status.ToLower().Contains("hoàn thành"))
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;

                chartData.Add(dayRevenue);
            }


            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartData = chartData;

            var model = new HomeAdminDashboardViewModel
            {
                OrdersToday = ordersToday,
                OrdersChange = ordersChange,
                RevenueToday = revenueToday,
                RevenueChange = revenueChange,
                NewCustomersToday = newCustomersToday,
                NewCustomersChange = newCustomersChange,
                PendingOrders = pendingOrders
            };
            var now = DateTime.Now;

            var saleProducts = db.Products
                .Include("ProductPromotions.Promotion")
                .Where(p => p.ProductPromotions.Any(pp =>
                    pp.Promotion.DisplayType == 2 &&
                    pp.Promotion.IsActive &&
                    now >= pp.Promotion.StartDate &&
                    now <= pp.Promotion.EndDate))
                .Select(p => new ProductWithSpecs
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    ImageURL = p.ImageURL,
                    Price = p.Price,
                    DiscountPercent = p.ProductPromotions
                                       .Where(pp => pp.Promotion.IsActive &&
                                                    now >= pp.Promotion.StartDate &&
                                                    now <= pp.Promotion.EndDate)
                                       .Select(pp => (decimal?)pp.Promotion.DiscountPercent)
                                       .FirstOrDefault(),
                    FinalPrice = p.Price * (100 -
                                 (p.ProductPromotions
                                    .Where(pp => pp.Promotion.IsActive &&
                                                 now >= pp.Promotion.StartDate &&
                                                 now <= pp.Promotion.EndDate)
                                    .Select(pp => (decimal?)pp.Promotion.DiscountPercent)
                                    .FirstOrDefault() ?? 0)) / 100

                })
                .Take(5)
                .ToList();

            model.SaleProducts = saleProducts;


            return View(model);
        }

        public class HomeAdminDashboardViewModel
        {
            public int OrdersToday { get; set; }
            public decimal OrdersChange { get; set; }

            public decimal RevenueToday { get; set; }
            public decimal RevenueChange { get; set; }

            public int NewCustomersToday { get; set; }
            public decimal NewCustomersChange { get; set; }

            public int PendingOrders { get; set; }
            public List<ProductWithSpecs> SaleProducts { get; set; }
        }
    }
}
