using Ecommerce.Helper;
using E_Commerce.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BC = BCrypt.Net;
using E_Commerce.Helper;

namespace Ecommerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyDbContext db;

        // Constructor - Inject DbContext
        public AccountController(MyDbContext context)
        {
            db = context;
        }

        // ============ ĐĂNG KÝ ============
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPost(IFormCollection fc)
        {
            string _Name = fc["name"];
            string _Email = fc["email"];
            string _Address = fc["address"];
            string _Phone = fc["phone"];
            string _Password = fc["password"];

            // Validate email format
            if (string.IsNullOrEmpty(_Email) || !_Email.Contains("@"))
            {
                TempData["Error"] = "Email không hợp lệ!";
                return Redirect("/Account/Register");
            }

            // Kiểm tra email đã tồn tại chưa
            var existingUser = db.Customers.FirstOrDefault(x => x.Email == _Email);
            if (existingUser != null)
            {
                TempData["Error"] = "Email đã được sử dụng!";
                return Redirect("/Account/Register");
            }

            // Hash password
            _Password = BC.BCrypt.HashPassword(_Password);

            // Tạo user mới (chưa verified)
            ItemCustomer record = new ItemCustomer();
            record.Name = _Name;
            record.Email = _Email;
            record.Address = _Address;
            record.Phone = _Phone;
            record.Password = _Password;
            db.Customers.Add(record);
            db.SaveChanges();

            // Tạo và lưu OTP
            Otp.SaveOtp(_Email, "register", 10); // Hết hạn sau 10 phút
            string otp = Otp.GetOtp(_Email, "register");

            // Gửi email OTP
            string emailBody = Email.RegisterOtpTemplate(_Name ?? "Khách hàng", otp);
            bool emailSent = await Email.SendEmail(_Email, "Xác thực tài khoản - Elegance Shop", emailBody);

            if (!emailSent)
            {
                // Nếu gửi email thất bại → Xóa user vừa tạo
                db.Customers.Remove(record);
                db.SaveChanges();

                TempData["Error"] = "Không thể gửi email xác thực. Email không tồn tại hoặc không hợp lệ!";
                return Redirect("/Account/Register");
            }

            // Gửi email thành công
            TempData["Success"] = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra!";
            TempData["RegisterEmail"] = _Email;

            return Redirect("/Account/VerifyRegisterOtp");
        }

        // ============ XÁC THỰC OTP ĐĂNG KÝ ============
        public IActionResult VerifyRegisterOtp()
        {
            // Lấy email từ TempData
            ViewBag.Email = TempData["RegisterEmail"];
            TempData.Keep("RegisterEmail"); // Giữ lại TempData
            return View();
        }

        [HttpPost]
        public IActionResult VerifyRegisterOtpPost(IFormCollection fc)
        {
            string email = fc["email"];
            string otp = fc["otp"];

            // Kiểm tra OTP
            bool isValid = Otp.ValidateOtp(email, "register", otp);

            if (!isValid)
            {
                TempData["Error"] = "Mã OTP không đúng hoặc đã hết hạn!";
                TempData["RegisterEmail"] = email;
                return Redirect("/Account/VerifyRegisterOtp");
            }

            // OTP hợp lệ → Cho phép đăng nhập
            TempData["Success"] = "Xác thực thành công! Bạn có thể đăng nhập ngay bây giờ.";
            return Redirect("/Account/Login");
        }

        // ============ ĐĂNG NHẬP ============
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginPost(IFormCollection formCollection)
        {
            string _email = formCollection["email"].ToString();
            string _password = formCollection["password"].ToString();
            bool rememberMe = formCollection["remember"] == "on"; // Check checkbox "Ghi nhớ"

            //lấy một bản ghi tương ứng với user truyền vào
            ItemCustomer user = db.Customers.FirstOrDefault(x => x.Email == _email);
            if (user == null)
            {
                TempData["Error"] = "Email hoặc mật khẩu không đúng!";
                return Redirect("/Account/Login");
            }

            //kiểm tra password (nếu user đăng ký bằng Google thì Password = null)
            if (user.Password != null && BC.BCrypt.Verify(_password, user.Password))
            {
                // KIỂM TRA THIẾT BỊ TIN CẬY
                if (TrustedDevice.IsTrustedDevice(HttpContext, _email))
                {
                    // Thiết bị đã tin cậy → Bỏ qua OTP, đăng nhập luôn
                    HttpContext.Session.SetString("customer_user_email", user.Email);
                    HttpContext.Session.SetString("customer_user_id", user.Id.ToString());
                    HttpContext.Session.SetString("customer_user_name", user.Name ?? "");

                    TempData["Success"] = "Đăng nhập thành công!";
                    return Redirect("/Home");
                }

                // Thiết bị mới → Gửi OTP
                Otp.SaveOtp(_email, "login", 5);
                string otp = Otp.GetOtp(_email, "login");

                string emailBody = Email.LoginOtpTemplate(user.Name ?? "Khách hàng", otp);
                bool emailSent = await Email.SendEmail(_email, "Mã đăng nhập - Elegance Shop", emailBody);

                if (!emailSent)
                {
                    TempData["Error"] = "Không thể gửi mã OTP. Vui lòng thử lại!";
                    return Redirect("/Account/Login");
                }

                // Lưu email và rememberMe vào TempData
                TempData["LoginEmail"] = _email;
                TempData["RememberMe"] = rememberMe;
                TempData["Success"] = "Mã OTP đã được gửi đến email của bạn!";

                return Redirect("/Account/VerifyLoginOtp");
            }
            else
            {
                TempData["Error"] = "Email hoặc mật khẩu không đúng!";
                return Redirect("/Account/Login");
            }
        }

        // ============ XÁC THỰC OTP ĐĂNG NHẬP ============
        public IActionResult VerifyLoginOtp()
        {
            ViewBag.Email = TempData["LoginEmail"];
            TempData.Keep("LoginEmail");
            return View();
        }

        [HttpPost]
        public IActionResult VerifyLoginOtpPost(IFormCollection fc)
        {
            string email = fc["email"];
            string otp = fc["otp"];
            bool rememberMe = TempData["RememberMe"] != null && (bool)TempData["RememberMe"];

            // Kiểm tra OTP
            bool isValid = Otp.ValidateOtp(email, "login", otp);

            if (!isValid)
            {
                TempData["Error"] = "Mã OTP không đúng hoặc đã hết hạn!";
                TempData["LoginEmail"] = email;
                TempData["RememberMe"] = rememberMe;
                return Redirect("/Account/VerifyLoginOtp");
            }

            // OTP hợp lệ → Đăng nhập thành công
            ItemCustomer user = db.Customers.FirstOrDefault(x => x.Email == email);

            if (user != null)
            {
                // Lưu session
                HttpContext.Session.SetString("customer_user_email", user.Email);
                HttpContext.Session.SetString("customer_user_id", user.Id.ToString());
                HttpContext.Session.SetString("customer_user_name", user.Name ?? "");

                // Lưu thiết bị tin cậy nếu user check "Ghi nhớ"
                if (rememberMe)
                {
                    TrustedDevice.SaveTrustedDevice(HttpContext, email, true);
                }

                TempData["Success"] = "Đăng nhập thành công!";
                return Redirect("/Home");
            }

            TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại!";
            return Redirect("/Account/Login");
        }

        // ============ ĐĂNG NHẬP GOOGLE ============
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Đăng nhập Google thất bại";
                return Redirect("/Account/Login");
            }

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims.ToList();
            //lấy thông tin từ google
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Không thể lấy thông tin Email từ Google";
                return Redirect("/Account/Login");
            }

            //Kiểm tra user đã tồn tại chưa
            var user = db.Customers.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                //Tạo user mới từ google (Google đã verify email rồi, không cần OTP)
                user = new ItemCustomer
                {
                    Name = name,
                    Email = email,
                    Password = null, //Không có mật khẩu vì đăng nhập bằng google
                    Address = "",
                    Phone = ""
                };
                db.Customers.Add(user);
                db.SaveChanges();
            }

            //Lưu session (Google login bỏ qua 2FA vì Google đã xác thực)
            HttpContext.Session.SetString("customer_user_email", user.Email);
            HttpContext.Session.SetString("customer_user_id", user.Id.ToString());
            HttpContext.Session.SetString("customer_user_name", user.Name ?? "");

            TempData["Success"] = "Đăng nhập Google thành công";
            return Redirect("/Home");
        }

        //Đăng xuất
        public async Task<IActionResult> Logout()
        {
            //hủy các biến session
            HttpContext.Session.Remove("customer_user_email");
            HttpContext.Session.Remove("customer_user_id");

            //Đăng xuất google
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }

        // ============ QUÊN MẬT KHẨU ============
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordPost(IFormCollection fc)
        {
            string _email = fc["email"].ToString();

            //Kiểm tra email xem có tồn tại trong database hay không
            ItemCustomer user = db.Customers.FirstOrDefault(c => c.Email == _email);
            if (user == null)
            {
                //email không tồn tại
                TempData["Error"] = "Email không tồn tại trong hệ thống!";
                return Redirect("/Account/ForgotPassword");
            }

            // Kiểm tra nếu user đăng ký bằng Google
            if (user.Password == null)
            {
                TempData["Error"] = "Tài khoản này đăng ký bằng Google, không có mật khẩu!";
                return Redirect("/Account/ForgotPassword");
            }

            // Tạo và gửi OTP
            Otp.SaveOtp(_email, "reset", 10); // Hết hạn sau 10 phút
            string otp = Otp.GetOtp(_email, "reset");

            string emailBody = Email.ResetPasswordOtpTemplate(user.Name ?? "Khách hàng", otp);
            bool emailSent = await Email.SendEmail(_email, "Đặt lại mật khẩu - Elegance Shop", emailBody);

            if (!emailSent)
            {
                TempData["Error"] = "Không thể gửi email. Vui lòng thử lại!";
                return Redirect("/Account/ForgotPassword");
            }

            TempData["Success"] = "Mã OTP đã được gửi đến email của bạn!";
            TempData["ResetEmail"] = _email;

            return Redirect("/Account/ResetPassword");
        }

        // ============ RESET PASSWORD ============
        public IActionResult ResetPassword()
        {
            ViewBag.Email = TempData["ResetEmail"];
            TempData.Keep("ResetEmail");
            return View();
        }

        [HttpPost]
        public IActionResult ResetPasswordPost(IFormCollection fc)
        {
            string email = fc["email"];
            string otp = fc["otp"];
            string newPassword = fc["password"];
            string confirmPassword = fc["confirmPassword"];

            // Kiểm tra password khớp
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                TempData["ResetEmail"] = email;
                return Redirect("/Account/ResetPassword");
            }

            // Kiểm tra OTP
            bool isValid = Otp.ValidateOtp(email, "reset", otp);

            if (!isValid)
            {
                TempData["Error"] = "Mã OTP không đúng hoặc đã hết hạn!";
                TempData["ResetEmail"] = email;
                return Redirect("/Account/ResetPassword");
            }

            // OTP hợp lệ → Đổi mật khẩu
            var user = db.Customers.FirstOrDefault(c => c.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Có lỗi xảy ra!";
                return Redirect("/Account/Login");
            }

            user.Password = BC.BCrypt.HashPassword(newPassword);
            db.SaveChanges();

            TempData["Success"] = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập ngay bây giờ.";
            return Redirect("/Account/Login");
        }

        public IActionResult Profile()
        {
            string email = HttpContext.Session.GetString("customer_user_email");
            if (string.IsNullOrEmpty(email))
                return Redirect("/Account/Login");
            return View();
        }

        [HttpGet]
        public IActionResult GetProfile()
        {
            string email = HttpContext.Session.GetString("customer_user_email");
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "Chưa đăng nhập" });

            var user  = db.Customers.FirstOrDefault(c=>c.Email == email);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy tài khoản" });

            return Ok(new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                phone = user.Phone,
                address = user.Address
            });
        }

        //Put
        [HttpPut]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            string email = HttpContext.Session.GetString("customer_user_email");
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "Chưa đăng nhập" });

            var user = db.Customers.FirstOrDefault(c => c.Email == email);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy tài khoản" });

            user.Name = request.Name;
            user.Phone = request.Phone;
            user.Address = request.Address;
            db.SaveChanges();

            return Ok(new { message = "Cập nhập thành công" });
        }
        public class UpdateProfileRequest   
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string Address{ get; set; }
        }

    }
}