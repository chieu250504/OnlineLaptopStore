using Laptop88_3.Models;
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Globalization;
using QRCoder;
using System.Drawing;
using System.IO;

namespace Laptop88_3.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private static string CalculateCRC(string data)
        {
            ushort crc = 0xFFFF;
            byte[] bytes = Encoding.ASCII.GetBytes(data);

            foreach (byte b in bytes)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                    crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
            }
            return crc.ToString("X4");
        }

        public ActionResult PaymentPage(int orderId)
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order == null) return RedirectToAction("Index", "Home");

            string bankAccount = "101877669109";
            string serviceType = "QRIBFTTA";
            string napasGUID = "A000000727";
            string currencyCode = "704";
            string amount = ((int)order.TotalAmount).ToString(CultureInfo.InvariantCulture);

            string content = $"Laptop88_Order{order.OrderID}";
            if (content.Length > 25) content = content.Substring(0, 25);

            
            string tag38_value =
                "0010" + napasGUID +
                "0126" + "00069704150112" + bankAccount + "0208" + serviceType;

            string tag38 = "38" + tag38_value.Length.ToString("D2") + tag38_value;

            
            string tag62_value = "08" + content.Length.ToString("D2") + content;
            string tag62 = "62" + tag62_value.Length.ToString("D2") + tag62_value;

            
            string payload =
                "000201" +
                "010212" +
                tag38 +
                $"5303{currencyCode}" +
                $"54{amount.Length:D2}{amount}" +
                "5802VN" +
                tag62 +
                "6304";

            string crc = CalculateCRC(payload);
            string qrString = payload + crc;

            ViewBag.QRString = qrString;
            ViewBag.OrderId = order.OrderID;
            ViewBag.Total = order.TotalAmount.ToString("N0");
            ViewBag.QRImage = GenerateQrCode(qrString);
            return View();
        }

        public ActionResult PaymentSuccess(int orderId)
        {
            var payment = db.Payments.FirstOrDefault(p => p.OrderID == orderId);
            if (payment != null)
            {
                payment.Status = "Paid";
                payment.PaidAt = DateTime.Now;
                db.SaveChanges();
            }

            return RedirectToAction("Success", "Checkout", new { orderId });
        }

        private string GenerateQrCode(string qrString)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrString, QRCodeGenerator.ECCLevel.Q))
                {
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        using (Bitmap qrBitmap = qrCode.GetGraphic(20))
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                string base64Image = Convert.ToBase64String(ms.ToArray());
                                return "data:image/png;base64," + base64Image;
                            }
                        }
                    }
                }
            }
        }
    }
}
