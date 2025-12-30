using Laptop88_3.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Entity;
namespace Laptop88_3.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly AppDbContext db = new AppDbContext(); 

        public ActionResult Revenue(string type = "day")
        {
            
            var orders = db.Orders
                .Where(o => o.Status.ToLower().Contains("hoàn thành"))
                .ToList();

            
            List<string> labels = new List<string>();
            List<decimal> data = new List<decimal>();

            if (type == "day")
            {
                
                var grouped = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .OrderByDescending(g => g.Key)
                    .Take(18)                     
                    .OrderBy(g => g.Key);

                foreach (var g in grouped)
                {
                    labels.Add(g.Key.ToString("dd/MM/yyyy"));
                    data.Add(g.Sum(o => o.TotalAmount));
                }
            }
            else if (type == "month")
            {
              
                var grouped = orders.Where(o => o.OrderDate.Year == DateTime.Now.Year)
                                    .GroupBy(o => o.OrderDate.Month)
                                    .OrderBy(g => g.Key);

                foreach (var g in grouped)
                {
                    labels.Add(g.Key.ToString() + "/" + DateTime.Now.Year);
                    data.Add(g.Sum(o => o.TotalAmount));
                }
            }
            else if (type == "year")
            {
                // Thống kê theo năm
                var grouped = orders.GroupBy(o => o.OrderDate.Year)
                                    .OrderBy(g => g.Key);

                foreach (var g in grouped)
                {
                    labels.Add(g.Key.ToString());
                    data.Add(g.Sum(o => o.TotalAmount));
                }
            }

            ViewBag.Labels = labels;
            ViewBag.Data = data;
            ViewBag.Type = type;

            return View();
        }
        public ActionResult ProductStatistics(string sort = "topsold")
        {
           
            var completedOrderDetails = db.OrderDetails
                .Include(od => od.Order)   
                .Include(od => od.Product) 
                .Where(od => od.Order.Status.Contains("hoàn thành") ||
                 od.Order.Status.Contains("dã hoàn thành") ||
                 od.Order.Status.Contains("hoan thanh"))
                .ToList();

            if (!completedOrderDetails.Any())
            {
                ViewBag.Message = "Chưa có dữ liệu OrderDetail hoàn thành!";
                ViewBag.Labels = new List<string>();
                ViewBag.Data = new List<decimal>();
                ViewBag.Sort = sort;
                return View();
            }

            var productStats = completedOrderDetails
                .GroupBy(od => od.ProductID)
                .Select(g => new
                {
                    ProductID = g.Key,
                    ProductName = g.FirstOrDefault().Product?.ProductName ?? "Unknown",
                    TotalSold = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .ToList();

           
            var topSold = productStats.OrderByDescending(p => p.TotalSold).Take(10).ToList();
            var lowSold = productStats.OrderBy(p => p.TotalSold).Take(10).ToList();
            var topRevenue = productStats.OrderByDescending(p => p.Revenue).Take(10).ToList();
            var lowRevenue = productStats.OrderBy(p => p.Revenue).Take(10).ToList();

            List<string> labels = new List<string>();
            List<decimal> data = new List<decimal>();

            switch (sort.ToLower())
            {
                case "topsold":
                    labels = topSold.Select(p => p.ProductName).ToList();
                    data = topSold.Select(p => (decimal)p.TotalSold).ToList();
                    break;
                case "lowsold":
                    labels = lowSold.Select(p => p.ProductName).ToList();
                    data = lowSold.Select(p => (decimal)p.TotalSold).ToList();
                    break;
                case "toprevenue":
                    labels = topRevenue.Select(p => p.ProductName).ToList();
                    data = topRevenue.Select(p => p.Revenue).ToList();
                    break;
                case "lowrevenue":
                    labels = lowRevenue.Select(p => p.ProductName).ToList();
                    data = lowRevenue.Select(p => p.Revenue).ToList();
                    break;
                default:
                    labels = topSold.Select(p => p.ProductName).ToList();
                    data = topSold.Select(p => (decimal)p.TotalSold).ToList();
                    break;
            }

            ViewBag.Labels = labels;
            ViewBag.Data = data;
            ViewBag.Sort = sort;
            ViewBag.Message = null;

            return View();
        }
    }
}
