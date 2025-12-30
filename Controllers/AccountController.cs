using Laptop88_3.Models;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Laptop88_3.Controllers
{
    public class AccountController : Controller
    {
        private AppDbContext db = new AppDbContext();

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            string passwordHash = ComputeSha256Hash(password);
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);

            if (user != null)
            {
                
                SignInUser(user, false);

                Session["UserID"] = user.UserID;
                Session["Username"] = user.Username;
                Session["Role"] = user.Role; 

                switch (user.Role)
                {
                    case "Admin":
                        return RedirectToAction("Home", "HomeAdmin");
                    default:
                        return RedirectToAction("Index", "Home"); 
                }
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }


        [HttpPost]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Clear();
            return RedirectToAction("Index", "Home");
            
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string Username, string Email, string FullName, string PhoneNumber,
                             string Address, string Password, string ConfirmPassword, string Otp)
        {
            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu không khớp.";
                return View();
            }
            if (Session["otp"] == null || Session["otpEmail"] == null)
            {
                ViewBag.Error = "Bạn chưa xác thực OTP.";
                return View();
            }

            if (Session["otp"].ToString() != Otp || Session["otpEmail"].ToString() != Email)
            {
                ViewBag.Error = "Mã OTP không đúng hoặc không khớp email.";
                return View();
            }

            bool emailExists = db.Users.Any(u => u.Email == Email);
            if (emailExists)
            {
                ViewBag.Error = "Email đã tồn tại. Vui lòng dùng email khác.";
                return View();
            }

            bool phoneExists = db.Users.Any(u => u.PhoneNumber == PhoneNumber);
            if (phoneExists)
            {
                ViewBag.Error = "Số điện thoại đã tồn tại. Vui lòng dùng số khác.";
                return View();
            }

            string passwordHash = ComputeSha256Hash(Password);
            var user = new User
            {
                Username = Username,
                Email = Email,
                FullName = FullName,
                PhoneNumber = PhoneNumber,
                Address = Address,
                PasswordHash = passwordHash,
                Role = "User",
                CreatedAt = DateTime.Now
            };

            db.Users.Add(user);
            db.SaveChanges();

            // Xóa OTP
            Session["otp"] = null;
            Session["otpEmail"] = null;

            ViewBag.RegisterSuccess = true;
            return View();

        }


        [HttpPost]
        public ActionResult SendOtp(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return Json(new { success = false, message = "Vui lòng nhập email!" });
            }

            string otp = GenerateOtp();
            Session["otp"] = otp;
            Session["otpEmail"] = Email;

            try
            {
                SendOtpEmail(Email, otp);
                System.Diagnostics.Debug.WriteLine("👉 OTP đã sinh: " + otp); // Debug
                return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Gửi OTP thất bại: " + ex.Message });
            }
        }

        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private void SendOtpEmail(string toEmail, string otp)
        {
            string fromEmail = "chieu25052004@gmail.com";
            string appPassword = "zhnj lfay jqou aklo"; 

            var mail = new MailMessage();
            mail.From = new MailAddress(fromEmail, "Laptop88");
            mail.To.Add(toEmail);
            mail.Subject = "Mã OTP xác thực từ Laptop88";
            mail.Body = $"Chào bạn,\n\nMã OTP của bạn là: {otp}\n\nLaptop88 kính chào!";
            mail.IsBodyHtml = false;

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail, appPassword)
            };

            client.Send(mail);
        }

        public ActionResult GoogleLogin()
        {
            return new ChallengeResult("Google", Url.Action("GoogleCallback", "Account"));
        }

        public ActionResult GoogleCallback()
        {
            var loginInfo = AuthenticationManager.GetExternalLoginInfo();
            if (loginInfo == null)
                return RedirectToAction("Login");

            var email = loginInfo.Email;
            var user = db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Username = email,
                    Email = email,
                    FullName = loginInfo.ExternalIdentity.FindFirstValue(ClaimTypes.Name),
                    Role = "User",
                    CreatedAt = DateTime.Now
                };
                db.Users.Add(user);
                db.SaveChanges();
            }

            SignInUser(user, true);
            Session["UserID"] = user.UserID;
            Session["Username"] = user.Username;
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string Email, string Otp, string NewPassword, string ConfirmPassword)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == Email);
            if (user == null)
            {
                ViewBag.Error = "Email chưa đăng ký!";
                return View();
            }

            if (!string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Otp))
            {
                string otp = GenerateOtp();
                Session["otp"] = otp;
                Session["otpEmail"] = Email;

                try
                {
                    SendOtpEmail(Email, otp);
                    ViewBag.Message = "OTP đã được gửi đến email của bạn.";
                }
                catch
                {
                    ViewBag.Error = "Gửi OTP thất bại!";
                }
                return View();
            }

            if (Session["otp"] == null || Session["otpEmail"] == null ||
                Session["otp"].ToString() != Otp || Session["otpEmail"].ToString() != Email)
            {
                ViewBag.Error = "OTP không hợp lệ hoặc không khớp email!";
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới không khớp!";
                return View();
            }

            user.PasswordHash = ComputeSha256Hash(NewPassword);
            db.SaveChanges();

            Session["otp"] = null;
            Session["otpEmail"] = null;

            ViewBag.Success = "Mật khẩu đã được cập nhật thành công!";
            ViewBag.PasswordChanged = true;
            return View();
        }


        private void SignInUser(User user, bool isPersistent)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            }, DefaultAuthenticationTypes.ApplicationCookie);

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var builder = new StringBuilder();
                foreach (var b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}
