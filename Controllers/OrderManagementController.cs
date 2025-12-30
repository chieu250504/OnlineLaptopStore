using Laptop88_3.Filters;
using Laptop88_3.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Laptop88_3.Controllers
{
    [AdminAuthorize]
    public class OrderManagementController : Controller
    {
        private readonly AppDbContext db;

        public OrderManagementController()
        {
            db = new AppDbContext();
        }
        public async Task<ActionResult> Index()
        {
            var orders = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var model = new List<OrderManagementViewModel>();

            
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://online-gateway.ghn.vn/");
                client.DefaultRequestHeaders.Add("Token", "0dcd3f5a-a2cb-11f0-9210-12d668528f01");
                client.DefaultRequestHeaders.Add("ShopId", "6049836");

                
                var tasks = orders.Select(async o =>
                {
                    string status = o.Status;

                    
                    if (!string.IsNullOrEmpty(o.ShippingCode) &&
                        !status.Contains("chờ duyệt") &&
                        !status.Contains("Hoàn thành") &&
                        !status.Contains("Đã hủy"))
                    {
                        var requestData = new { order_code = o.ShippingCode };
                        var json = JsonConvert.SerializeObject(requestData);
                        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                        var response = await client.PostAsync("shiip/public-api/v2/shipping-order/detail", content);
                        var result = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            dynamic data = JsonConvert.DeserializeObject(result);
                            if (data.code == 200 && data.data != null)
                            {
                                status = data.data.status;
                            }
                        }
                    }

                    return new OrderManagementViewModel
                    {
                        OrderID = o.OrderID,
                        OrderDate = o.OrderDate,
                        Username = o.User?.Username ?? "(Không xác định)",
                        Status = status,
                        ShippingAddress = o.ShippingAddress,
                        PhoneShipping = o.PhoneShipping,
                        TotalAmount = o.TotalAmount,
                        ShippingCode = o.ShippingCode,
                        Items = o.OrderDetails.Select(d => new OrderItemViewModel
                        {
                            ProductName = d.Product?.ProductName ?? "(Sản phẩm không tồn tại)",
                            Quantity = d.Quantity,
                            UnitPrice = d.UnitPrice,
                            ImageURL = d.Product?.ImageURL,
                            IsGift = d.UnitPrice == 0m
                        }).ToList()
                    };
                }).ToList();

                
                model = (await Task.WhenAll(tasks)).ToList();
            }

            return View(model);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }
        public async Task<ActionResult> ApproveOrder(int id)
        {
            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o => o.OrderID == id);
            if (order == null)
                return HttpNotFound();

            var items = order.OrderDetails.Select(i => new
            {
                name = i.Product.ProductName,
                code = i.Product.ProductID,
                quantity = i.Quantity,
                price = (int)i.UnitPrice,
                weight = i.Product.Weight,   
                length = i.Product.Length,   
                width = i.Product.Width,     
                height = i.Product.Height,   
                category = new { level1 = "Khác" } 
            }).ToList();


            var requestData = new
            {
                payment_type_id = 2,
                note = "Giao hàng",
                required_note = "KHONGCHOXEMHANG",
                return_phone = "0335453298",
                return_address = "39 NTT",
                return_district_id = 1444,
                return_ward_code = "20308",
                to_name = order.User.FullName,
                to_phone = order.PhoneShipping,
                to_address = order.ShippingAddress,
                to_ward_code = order.WardCode,
                to_district_id = Convert.ToInt32(order.DistrictCode),
                cod_amount = Convert.ToInt32(order.TotalAmount),
                content = "Đơn hàng Laptop88",
                weight = 200,
                length = 10,
                width = 15,
                height = 5,
                insurance_value = Convert.ToInt32(order.TotalAmount),
                service_type_id = 2,
                pick_shift = new[] { 2 },
                items
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://online-gateway.ghn.vn/");

                client.DefaultRequestHeaders.Add("Token", "0dcd3f5a-a2cb-11f0-9210-12d668528f01");
                client.DefaultRequestHeaders.Add("ShopId", "6049836");

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("shiip/public-api/v2/shipping-order/create", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic data = JsonConvert.DeserializeObject(result);

                    order.ShippingCode = data.data.order_code;
                    order.Status = "Đã tạo vận đơn";
                    db.SaveChanges();

                    TempData["success"] = "Tạo vận đơn thành công!";
                }
                else
                {
                    TempData["error"] = "Lỗi tạo vận đơn: " + result;
                }
            }

            return RedirectToAction("Index");
        }
        public async Task<ActionResult> CancelOrder(int id)
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderID == id);
            if (order == null)
                return HttpNotFound();

            if (string.IsNullOrEmpty(order.ShippingCode))
            {
                TempData["error"] = "Đơn hàng chưa có mã vận đơn, không thể hủy.";
                return RedirectToAction("Index");
            }

            var requestData = new
            {
                order_codes = new[] { order.ShippingCode } 
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://online-gateway.ghn.vn/");

                client.DefaultRequestHeaders.Add("Token", "0dcd3f5a-a2cb-11f0-9210-12d668528f01");
                client.DefaultRequestHeaders.Add("ShopId", "6049836");

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("shiip/public-api/v2/switch-status/cancel", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic data = JsonConvert.DeserializeObject(result);

                    order.Status = "Đã hủy vận đơn";
                    db.SaveChanges();

                    TempData["success"] = "Hủy đơn thành công!";
                }
                else
                {
                    TempData["error"] = "Lỗi hủy đơn: " + result;
                }
            }

            return RedirectToAction("Index");
        }
        public async Task<ActionResult> DetailOrder(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
                return HttpNotFound();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://online-gateway.ghn.vn/");
                client.DefaultRequestHeaders.Add("Token", "0dcd3f5a-a2cb-11f0-9210-12d668528f01");
                client.DefaultRequestHeaders.Add("ShopId", "6049836");

                var requestData = new { order_code = orderCode };
                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("shiip/public-api/v2/shipping-order/detail", content);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TempData["error"] = "Không thể lấy chi tiết đơn: " + result;
                    return RedirectToAction("Index");
                }

                dynamic data = JsonConvert.DeserializeObject(result);

                if (data.code != 200 || data.data == null)
                {
                    TempData["error"] = "Không có dữ liệu chi tiết đơn hàng!";
                    return RedirectToAction("Index");
                }

                var orderData = data.data;

                var model = new ShippingOrderDetailViewModel
                {
                    OrderCode = orderData.order_code,
                    Status = orderData.status,
                    ToName = orderData.to_name,
                    ToPhone = orderData.to_phone,
                    ToAddress = orderData.to_address,
                    ShippingNote = orderData.note,
                    Weight = orderData.weight,
                    Items = new List<ShippingOrderItemViewModel>(),
                    Log = new List<ShippingOrderLogViewModel>()
                };

                
                if (orderData.items != null)
                {
                    foreach (var item in orderData.items)
                    {
                        model.Items.Add(new ShippingOrderItemViewModel
                        {
                            Name = item.name,
                            Quantity = item.quantity,
                            Weight = item.weight
                        });
                    }
                }

                
                if (orderData.log != null)
                {
                    foreach (var log in orderData.log)
                    {
                        model.Log.Add(new ShippingOrderLogViewModel
                        {
                            Status = log.status,
                            UpdatedDate = log.updated_date
                        });
                    }
                }


                return View(model);
            }
        }
        public async Task<ActionResult> PrintOrder(string orderCode, string format = "A5")
        {
            if (string.IsNullOrEmpty(orderCode))
                return HttpNotFound();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://online-gateway.ghn.vn/");
                client.DefaultRequestHeaders.Add("Token", "0dcd3f5a-a2cb-11f0-9210-12d668528f01");
                

                var requestData = new { order_codes = new[] { orderCode } };
                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("shiip/public-api/v2/a5/gen-token", content);
                var result = await response.Content.ReadAsStringAsync();

                dynamic data = JsonConvert.DeserializeObject(result);

                if (data.code != 200 || data.data.token == null)
                {
                    TempData["error"] = "Không thể lấy token in đơn: " + result;
                    return RedirectToAction("DetailOrder", new { orderCode });
                }

                string token = data.data.token;
                string url = "";

                
                if (format == "A5")
                {
                    url = $"https://online-gateway.ghn.vn/a5/public-api/printA5?token={token}&order_code={orderCode}";
                }
                else if (format == "80x80")
                {
                    url = $"https://online-gateway.ghn.vn/a5/public-api/print80x80?token={token}&order_code={orderCode}";
                }
                else if (format == "50x72")
                {
                    url = $"https://online-gateway.ghn.vn/a5/public-api/print52x70?token={token}&order_code={orderCode}";
                }
                else
                {
                    // Mặc định A5
                    url = $"https://online-gateway.ghn.vn/a5/public-api/printA5?token={token}&order_code={orderCode}";
                }

                return Redirect(url);
            }
        }

        public ActionResult PendingOrders()
        {
            
            var orders = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .Where(o => o.Status != null && o.Status.Contains("chờ duyệt"))
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var model = orders.Select(o => new OrderManagementViewModel
            {
                OrderID = o.OrderID,
                OrderDate = o.OrderDate,
                Username = o.User?.Username ?? "(Không xác định)",
                Status = o.Status,
                ShippingAddress = o.ShippingAddress,
                PhoneShipping = o.PhoneShipping,
                TotalAmount = o.TotalAmount,
                ShippingCode = o.ShippingCode,
                Items = o.OrderDetails.Select(d => new OrderItemViewModel
                {
                    ProductName = d.Product?.ProductName ?? "(Sản phẩm không tồn tại)",
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    ImageURL = d.Product?.ImageURL,
                    IsGift = d.UnitPrice == 0m
                }).ToList()
            }).ToList();

            return View("Index", model); 
        }
        public ActionResult ApprovedOrders()
        {
            var orders = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .Where(o => o.Status != null && o.Status.Contains("Hoàn thành")) 
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var model = orders.Select(o => new OrderManagementViewModel
            {
                OrderID = o.OrderID,
                OrderDate = o.OrderDate,
                Username = o.User?.Username ?? "(Không xác định)",
                Status = o.Status,
                ShippingAddress = o.ShippingAddress,
                PhoneShipping = o.PhoneShipping,
                TotalAmount = o.TotalAmount,
                ShippingCode = o.ShippingCode,
                Items = o.OrderDetails.Select(d => new OrderItemViewModel
                {
                    ProductName = d.Product?.ProductName ?? "(Sản phẩm không tồn tại)",
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    ImageURL = d.Product?.ImageURL,
                    IsGift = d.UnitPrice == 0m
                }).ToList()
            }).ToList();

            return View("Index", model);
        }
        public ActionResult CancelledOrders()
        {
            var orders = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .Where(o => o.Status != null && o.Status.Contains("Đã hủy")) 
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var model = orders.Select(o => new OrderManagementViewModel
            {
                OrderID = o.OrderID,
                OrderDate = o.OrderDate,
                Username = o.User?.Username ?? "(Không xác định)",
                Status = o.Status,
                ShippingAddress = o.ShippingAddress,
                PhoneShipping = o.PhoneShipping,
                TotalAmount = o.TotalAmount,
                ShippingCode = o.ShippingCode,
                Items = o.OrderDetails.Select(d => new OrderItemViewModel
                {
                    ProductName = d.Product?.ProductName ?? "(Sản phẩm không tồn tại)",
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    ImageURL = d.Product?.ImageURL,
                    IsGift = d.UnitPrice == 0m
                }).ToList()
            }).ToList();

            return View("Index", model);
        }

    }

    public class OrderManagementViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneShipping { get; set; }
        public string ShippingCode { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; set; }
        public string ImageURL { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsGift { get; set; }
    }
    public class ShippingOrderDetailViewModel
    {
        public string OrderCode { get; set; }
        public string Status { get; set; }
        public string ToName { get; set; }
        public string ToPhone { get; set; }
        public string ToAddress { get; set; }
        public string ShippingNote { get; set; }
        public decimal Weight { get; set; }
        public List<ShippingOrderItemViewModel> Items { get; set; } = new List<ShippingOrderItemViewModel>();
        public List<ShippingOrderLogViewModel> Log { get; set; } = new List<ShippingOrderLogViewModel>();
    }

    public class ShippingOrderItemViewModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Weight { get; set; }
    }

    public class ShippingOrderLogViewModel
    {
        public string Status { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
