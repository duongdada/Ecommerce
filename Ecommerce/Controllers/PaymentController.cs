using E_Commerce.Helper;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace E_Commerce.Controllers
{
    public class PaymentController : Controller
    {
        private readonly MyDbContext db = new MyDbContext();

        // ============ XỬ LÝ THANH TOÁN CHUNG ============
        public async Task<IActionResult> Process(string method)
        {
            // Lấy thông tin từ Session
            string name = HttpContext.Session.GetString("checkout_name");
            string email = HttpContext.Session.GetString("checkout_email");
            string phone = HttpContext.Session.GetString("checkout_phone");
            string address = HttpContext.Session.GetString("checkout_address");
            string paymentMethod = HttpContext.Session.GetString("checkout_payment");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Phiên làm việc hết hạn. Vui lòng thử lại!";
                return Redirect("/Cart/Checkout");
            }

            List<Item> cart = Cart.GetCart(HttpContext.Session);
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return Redirect("/Cart");
            }

            string orderCode = "";
            string userIdStr = HttpContext.Session.GetString("customer_user_id");

            // ⭐ CHỈ TẠO ĐƠN HÀNG KHI THANH TOÁN COD
            // Với MoMo/ZaloPay → Tạo đơn hàng SAU KHI thanh toán thành công
            if (paymentMethod == "COD")
            {
                // Tạo đơn hàng ngay
                if (!string.IsNullOrEmpty(userIdStr))
                {
                    int customerId = int.Parse(userIdStr);
                    Cart.CartCheckOut(HttpContext.Session, customerId, paymentMethod);

                    var lastOrder = db.Orders
                        .Where(o => o.CustomerId == customerId)
                        .OrderByDescending(o => o.Id)
                        .FirstOrDefault();
                    orderCode = lastOrder?.OrderCode;
                }
                else
                {
                    orderCode = Cart.CartCheckOutGuest(HttpContext.Session, name, email, phone, address, paymentMethod);
                }

                if (string.IsNullOrEmpty(orderCode))
                {
                    TempData["Error"] = "Có lỗi xảy ra khi đặt hàng!";
                    return Redirect("/Cart/Checkout");
                }

                // Gửi email xác nhận
                var order = db.Orders.FirstOrDefault(o => o.OrderCode == orderCode);
                var orderDetails = db.OrdersDetail.Where(od => od.OrderId == order.Id).ToList();

                string emailBody = Email.OrderConfirmationTemplate(order, orderDetails);
                await Email.SendEmail(email, $"Xác nhận đơn hàng #{orderCode} - Elegance Shop", emailBody);

                // Xóa thông tin checkout khỏi Session
                ClearCheckoutSession();

                TempData["OrderCode"] = orderCode;
                TempData["Email"] = email;
                return Redirect("/Cart/CheckoutSuccess");
            }
            else if (paymentMethod == "MoMo")
            {
                // ⭐ CHƯA TẠO ĐƠN HÀNG - Chỉ chuyển sang trang thanh toán
                // Session vẫn giữ thông tin để tạo đơn SAU KHI thanh toán thành công
                return Redirect("/Payment/MoMoCheckout");
            }
            else if (paymentMethod == "ZaloPay")
            {
                // ⭐ CHƯA TẠO ĐƠN HÀNG - Chỉ chuyển sang trang thanh toán
                return Redirect("/Payment/ZaloPayCheckout");
            }

            TempData["Error"] = "Phương thức thanh toán không hợp lệ!";
            return Redirect("/Cart/Checkout");
        }

        // ============ MOMO PAYMENT ============
        public IActionResult MoMoCheckout()
        {
            // Lấy thông tin từ Session
            string name = HttpContext.Session.GetString("checkout_name");
            string email = HttpContext.Session.GetString("checkout_email");
            string phone = HttpContext.Session.GetString("checkout_phone");
            string address = HttpContext.Session.GetString("checkout_address");

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Phiên làm việc hết hạn!";
                return Redirect("/Cart/Checkout");
            }

            List<Item> cart = Cart.GetCart(HttpContext.Session);
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return Redirect("/Cart");
            }

            // ⭐ TẠO ĐƠN HÀNG TẠM (để có OrderCode cho MoMo)
            string orderCode = "";
            string userIdStr = HttpContext.Session.GetString("customer_user_id");

            if (!string.IsNullOrEmpty(userIdStr))
            {
                int customerId = int.Parse(userIdStr);
                Cart.CartCheckOut(HttpContext.Session, customerId, "MoMo");

                var lastOrder = db.Orders
                    .Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.Id)
                    .FirstOrDefault();
                orderCode = lastOrder?.OrderCode;
            }
            else
            {
                orderCode = Cart.CartCheckOutGuest(HttpContext.Session, name, email, phone, address, "MoMo");
            }

            if (string.IsNullOrEmpty(orderCode))
            {
                TempData["Error"] = "Có lỗi xảy ra khi tạo đơn hàng!";
                return Redirect("/Cart/Checkout");
            }

            // Lấy thông tin đơn hàng
            var order = db.Orders.FirstOrDefault(o => o.OrderCode == orderCode);
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại!";
                return Redirect("/Cart");
            }

            // Tạo URL thanh toán MoMo
            string paymentUrl = MoMoPayment.CreatePaymentUrl(
                orderCode,
                order.Price,
                $"Thanh toán đơn hàng #{orderCode}"
            );

            if (string.IsNullOrEmpty(paymentUrl))
            {
                TempData["Error"] = "Không thể kết nối đến MoMo. Vui lòng thử lại!";
                return Redirect("/Cart/Checkout");
            }

            // Redirect đến trang thanh toán MoMo
            return Redirect(paymentUrl);
        }

        // Callback từ MoMo sau khi thanh toán
        [HttpGet]
        public async Task<IActionResult> MoMoReturn()
        {
            // Lấy các tham số từ URL query string
            string partnerCode = Request.Query["partnerCode"];
            string orderId = Request.Query["orderId"];
            string requestId = Request.Query["requestId"];
            string amount = Request.Query["amount"];
            string orderInfo = Request.Query["orderInfo"];
            string orderType = Request.Query["orderType"];
            string transId = Request.Query["transId"];
            string resultCode = Request.Query["resultCode"];
            string message = Request.Query["message"];
            string payType = Request.Query["payType"];
            string responseTime = Request.Query["responseTime"];
            string extraData = Request.Query["extraData"];
            string signature = Request.Query["signature"];

            // Verify signature
            string rawHash = $"accessKey={MoMoPayment.GetAccessKey()}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&partnerCode={partnerCode}&payType={payType}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}&transId={transId}";

            bool isValidSignature = MoMoPayment.VerifySignature(rawHash, signature);

            if (!isValidSignature)
            {
                TempData["Error"] = "Chữ ký không hợp lệ!";
                return Redirect("/Cart/Checkout");
            }

            // Kiểm tra kết quả thanh toán
            if (resultCode == "0") // Thanh toán thành công
            {
                var order = db.Orders.FirstOrDefault(o => o.OrderCode == orderId);
                if (order != null)
                {
                    // Cập nhật trạng thái thanh toán
                    order.PaymentStatus = 1; // Đã thanh toán
                    order.TransactionId = transId;
                    db.SaveChanges();

                    // Gửi email xác nhận
                    var orderDetails = db.OrdersDetail.Where(od => od.OrderId == order.Id).ToList();
                    string emailBody = Email.OrderConfirmationTemplate(order, orderDetails);
                    await Email.SendEmail(order.Email, $"Xác nhận đơn hàng #{orderId} - Elegance Shop", emailBody);

                    // Xóa thông tin checkout khỏi Session
                    ClearCheckoutSession();

                    TempData["OrderCode"] = orderId;
                    TempData["Email"] = order.Email;
                    return Redirect("/Cart/CheckoutSuccess");
                }
            }
            else // Thanh toán thất bại
            {
                // ⭐ XÓA ĐƠN HÀNG CHƯA THANH TOÁN
                var order = db.Orders.FirstOrDefault(o => o.OrderCode == orderId);
                if (order != null)
                {
                    var orderDetails = db.OrdersDetail.Where(od => od.OrderId == order.Id).ToList();
                    db.OrdersDetail.RemoveRange(orderDetails);
                    db.Orders.Remove(order);
                    db.SaveChanges();
                }

                TempData["Error"] = $"Thanh toán thất bại: {message}";
                return Redirect("/Cart/Checkout");
            }

            return Redirect("/Cart");
        }

        // IPN Callback từ MoMo (Server-to-Server)
        [HttpPost]
        public IActionResult MoMoNotify()
        {
            // Đọc raw body
            string body = "";
            using (var reader = new StreamReader(Request.Body))
            {
                body = reader.ReadToEndAsync().Result;
            }

            dynamic data = JsonConvert.DeserializeObject(body);

            // Verify và xử lý tương tự như MoMoReturn
            // ...

            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }

        // ============ ZALOPAY PAYMENT ============
        public IActionResult ZaloPayCheckout()
        {
            // Lấy thông tin từ Session
            string name = HttpContext.Session.GetString("checkout_name");
            string email = HttpContext.Session.GetString("checkout_email");
            string phone = HttpContext.Session.GetString("checkout_phone");
            string address = HttpContext.Session.GetString("checkout_address");

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Phiên làm việc hết hạn!";
                return Redirect("/Cart/Checkout");
            }

            List<Item> cart = Cart.GetCart(HttpContext.Session);
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return Redirect("/Cart");
            }

            // ⭐ TẠO ĐƠN HÀNG TẠM (để có OrderCode cho ZaloPay)
            string orderCode = "";
            string userIdStr = HttpContext.Session.GetString("customer_user_id");

            if (!string.IsNullOrEmpty(userIdStr))
            {
                int customerId = int.Parse(userIdStr);
                Cart.CartCheckOut(HttpContext.Session, customerId, "ZaloPay");

                var lastOrder = db.Orders
                    .Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.Id)
                    .FirstOrDefault();
                orderCode = lastOrder?.OrderCode;
            }
            else
            {
                orderCode = Cart.CartCheckOutGuest(HttpContext.Session, name, email, phone, address, "ZaloPay");
            }

            if (string.IsNullOrEmpty(orderCode))
            {
                TempData["Error"] = "Có lỗi xảy ra khi tạo đơn hàng!";
                return Redirect("/Cart/Checkout");
            }

            // Lấy thông tin đơn hàng
            var order = db.Orders.FirstOrDefault(o => o.OrderCode == orderCode);
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại!";
                return Redirect("/Cart");
            }

            // Tạo URL thanh toán ZaloPay
            string paymentUrl = ZaloPayPayment.CreatePaymentUrl(
                orderCode,
                order.Price,
                $"Thanh toán đơn hàng #{orderCode}"
            );

            if (string.IsNullOrEmpty(paymentUrl))
            {
                TempData["Error"] = "Không thể kết nối đến ZaloPay. Vui lòng thử lại!";
                return Redirect("/Cart/Checkout");
            }

            // Redirect đến trang thanh toán ZaloPay
            return Redirect(paymentUrl);
        }

        // Callback từ ZaloPay sau khi thanh toán
        [HttpGet]
        public async Task<IActionResult> ZaloPayReturn()
        {
            string status = Request.Query["status"];
            string appTransId = Request.Query["apptransid"];

            if (status == "1") // Thanh toán thành công
            {
                // Lấy orderCode từ appTransId (format: yyMMdd_orderCode)
                string orderCode = appTransId.Split('_').Last();

                var order = db.Orders.FirstOrDefault(o => o.OrderCode == orderCode);
                if (order != null)
                {
                    // Cập nhật trạng thái thanh toán
                    order.PaymentStatus = 1; // Đã thanh toán
                    order.TransactionId = appTransId;
                    db.SaveChanges();

                    // Gửi email xác nhận
                    var orderDetails = db.OrdersDetail.Where(od => od.OrderId == order.Id).ToList();
                    string emailBody = Email.OrderConfirmationTemplate(order, orderDetails);
                    await Email.SendEmail(order.Email, $"Xác nhận đơn hàng #{orderCode} - Elegance Shop", emailBody);

                    // Xóa thông tin checkout khỏi Session
                    ClearCheckoutSession();

                    TempData["OrderCode"] = orderCode;
                    TempData["Email"] = order.Email;
                    return Redirect("/Cart/CheckoutSuccess");
                }
            }
            else // Thanh toán thất bại hoặc bị hủy
            {
                // ⭐ XÓA ĐƠN HÀNG CHƯA THANH TOÁN
                if (!string.IsNullOrEmpty(appTransId))
                {
                    string orderCode = appTransId.Split('_').Last();
                    var order = db.Orders.FirstOrDefault(o => o.OrderCode == orderCode);
                    if (order != null)
                    {
                        var orderDetails = db.OrdersDetail.Where(od => od.OrderId == order.Id).ToList();
                        db.OrdersDetail.RemoveRange(orderDetails);
                        db.Orders.Remove(order);
                        db.SaveChanges();
                    }
                }

                TempData["Error"] = "Thanh toán thất bại hoặc đã bị hủy!";
                return Redirect("/Cart/Checkout");
            }

            return Redirect("/Cart");
        }

        // Server callback từ ZaloPay
        [HttpPost]
        public IActionResult ZaloPayCallback()
        {
            var result = new { return_code = 1, return_message = "" };

            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = reader.ReadToEndAsync().Result;
                    dynamic cbData = JsonConvert.DeserializeObject(body);

                    string dataStr = Convert.ToString(cbData.data);
                    string reqMac = Convert.ToString(cbData.mac);

                    // Verify callback
                    bool isValid = ZaloPayPayment.VerifyCallback(dataStr, reqMac);

                    if (isValid)
                    {
                        dynamic data = JsonConvert.DeserializeObject(dataStr);
                        string appTransId = data.app_trans_id;
                        string orderCode = appTransId.Split('_').Last();

                        var order = db.Orders.FirstOrDefault(o => o.OrderCode == orderCode);
                        if (order != null)
                        {
                            order.PaymentStatus = 1;
                            order.TransactionId = appTransId;
                            db.SaveChanges();
                        }

                        result = new { return_code = 1, return_message = "success" };
                    }
                    else
                    {
                        result = new { return_code = -1, return_message = "mac not equal" };
                    }
                }
            }
            catch (Exception ex)
            {
                result = new { return_code = 0, return_message = ex.Message };
            }

            return Json(result);
        }

        // ⭐ HELPER: Xóa thông tin checkout khỏi Session
        private void ClearCheckoutSession()
        {
            HttpContext.Session.Remove("checkout_name");
            HttpContext.Session.Remove("checkout_email");
            HttpContext.Session.Remove("checkout_phone");
            HttpContext.Session.Remove("checkout_address");
            HttpContext.Session.Remove("checkout_payment");
        }
    }
}