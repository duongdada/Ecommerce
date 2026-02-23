using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    public class OrderController : Controller
    {
        public MyDbContext db = new MyDbContext();

        // ============ TRANG TRA CỨU ĐƠN HÀNG ============
        public IActionResult Track()
        {
            return View();
        }

        // ============ XỬ LÝ TRA CỨU ============
        [HttpPost]
        public IActionResult TrackPost(IFormCollection fc)
        {
            string email = fc["email"];
            string orderCode = fc["orderCode"];

            // Validate
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(orderCode))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ Email và Mã đơn hàng!";
                return Redirect("/Order/Track");
            }

            // Tìm đơn hàng
            var order = db.Orders.FirstOrDefault(o =>
                o.Email == email &&
                o.OrderCode == orderCode);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng! Vui lòng kiểm tra lại Email và Mã đơn hàng.";
                return Redirect("/Order/Track");
            }

            // Chuyển đến trang chi tiết
            return Redirect($"/Order/Detail/{order.Id}");
        }

        // ============ CHI TIẾT ĐƠN HÀNG ============
        public IActionResult Detail(int id)
        {
            var order = db.Orders.FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng!";
                return Redirect("/Order/Track");
            }

            // Lấy chi tiết sản phẩm
            var orderDetails = db.OrdersDetail.Where(od => od.OrderId == id).ToList();

            ViewBag.Order = order;
            ViewBag.OrderDetails = orderDetails;

            return View();
        }
    }
}