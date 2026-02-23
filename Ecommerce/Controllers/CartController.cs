using E_Commerce.Models;
using E_Commerce.Helper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace E_Commerce.Controllers
{
    public class CartController : Controller
    {
        public MyDbContext db = new MyDbContext();

        public IActionResult Index()
        {
            //Lấy chuỗi json
            string json_cart = HttpContext.Session.GetString("cart");
            //Tạo biến cart để chuẩn bị đổ dữ liệu từ biến json vào
            List<Item> cart = new List<Item>();
            if (!String.IsNullOrEmpty(json_cart))
            {
                //chuyển chuỗi json ra thành dạnh list
                cart = JsonConvert.DeserializeObject<List<Item>>(json_cart);
            }
            return View("Index", cart);
        }

        //cho sản phẩm vào giỏ hàng (bằng chuỗi json)
        public IActionResult Buy(int id)
        {
            //gọi hàm Add từ class cart
            Cart.CartAdd(HttpContext.Session, id);
            return RedirectToAction("Index");
        }

        //xoá sản phẩm khỏi giỏ hàng
        public IActionResult Remove(int id)
        {
            Cart.CartRemove(HttpContext.Session, id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        //Cập nhập số lượng sản phẩm
        public IActionResult Update()
        {
            //Lấy chuỗi json
            string json_cart = HttpContext.Session.GetString("cart");
            //Tạo biến cart để chuẩn bị đổ dữ liệu từ biến json vào
            List<Item> cart = new List<Item>();
            if (!String.IsNullOrEmpty(json_cart))
            {
                //chuyển chuỗi json ra thành dạnh list
                cart = JsonConvert.DeserializeObject<List<Item>>(json_cart);
            }
            //--
            //duyệt các item trong list cart để update số lượng
            foreach (var product in cart)
            {
                int quantity = Convert.ToInt32(Request.Form["product_" + product.ProductRecord.Id]);
                //gọi hàm CartUpdate để update số lượng
                Cart.CartUpdate(HttpContext.Session, product.ProductRecord.Id, quantity);
            }
            //--
            return Redirect("/Cart");
        }

        //xoá toàn bộ sản phẩm trong giỏ hàng
        public IActionResult Destroy()
        {
            Cart.CartDestroy(HttpContext.Session);
            return RedirectToAction("Index");
        }

        // ============ CHECKOUT - Hiển thị trang thanh toán ============
        public IActionResult Checkout()
        {
            List<Item> cart = Cart.GetCart(HttpContext.Session);

            // Kiểm tra giỏ hàng trống
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return Redirect("/Cart");
            }

            // Nếu đã đăng nhập → Lấy thông tin user để auto-fill
            string userEmail = HttpContext.Session.GetString("customer_user_email");
            if (!string.IsNullOrEmpty(userEmail))
            {
                var customer = db.Customers.FirstOrDefault(c => c.Email == userEmail);
                ViewBag.Customer = customer;
            }

            return View(cart);
        }

        // ============ XỬ LÝ CHECKOUT - CHUYỂN SANG PAYMENT CONTROLLER ============
        [HttpPost]
        public IActionResult CheckoutPost(IFormCollection fc)
        {
            string name = fc["name"];
            string email = fc["email"];
            string phone = fc["phone"];
            string address = fc["address"];
            string paymentMethod = fc["paymentMethod"];

            // Validate
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(address))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin!";
                return Redirect("/Cart/Checkout");
            }

            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["Error"] = "Vui lòng chọn phương thức thanh toán!";
                return Redirect("/Cart/Checkout");
            }

            List<Item> cart = Cart.GetCart(HttpContext.Session);
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return Redirect("/Cart");
            }

            // Lưu thông tin vào Session để PaymentController sử dụng
            HttpContext.Session.SetString("checkout_name", name);
            HttpContext.Session.SetString("checkout_email", email);
            HttpContext.Session.SetString("checkout_phone", phone);
            HttpContext.Session.SetString("checkout_address", address);
            HttpContext.Session.SetString("checkout_payment", paymentMethod);

            // ⭐ CHUYỂN SANG PAYMENT CONTROLLER
            return Redirect($"/Payment/Process?method={paymentMethod}");
        }

        // ============ TRANG THÀNH CÔNG ============
        public IActionResult CheckoutSuccess()
        {
            ViewBag.OrderCode = TempData["OrderCode"];
            ViewBag.Email = TempData["Email"];
            return View();
        }
    }
}