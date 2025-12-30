using System;
using System.Linq;
using System.Web.Mvc;
using Laptop88_3.Models;
using System.Data.Entity;

namespace Laptop88_3.Controllers
{
    public class OrderController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            var orders = db.Orders.Include(o => o.User).ToList();
            return View(orders);
        }
        public ActionResult Details(int id)
        {
            var order = db.Orders
                          .Include(o => o.User)
                          .Include(o => o.OrderDetails.Select(d => d.Product))
                          .Include(o => o.OrderDetails.Select(d => d.Product.ProductPromotions.Select(pp => pp.Promotion)))
                          .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
                return HttpNotFound();

            return View(order);
        }
    }
}
