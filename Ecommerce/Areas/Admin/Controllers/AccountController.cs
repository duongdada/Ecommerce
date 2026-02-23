using Microsoft.AspNetCore.Mvc;
using E_Commerce.Models;
using BC = BCrypt.Net;

namespace E_Commerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private MyDbContext db  =  new MyDbContext();
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult LoginPost(IFormCollection formCollection)
        {
            string _email = formCollection["email"].ToString();
            string _password = formCollection["password"].ToString();

            //Lấy 1 bản ghi tương ứng với user truyền vào 
            RowUser user = db.Users.FirstOrDefault(c=>c.Email==_email);
            if (user == null) //nếu như user == rỗng
            {
                //di chuyển đến url /Admin/Acount/login
                return Redirect("/Admin/Account/Login");
            }
            else
            {
                //Kiểm tra Password
                if(BC.BCrypt.Verify(_password, user.Password))
                {
                    //Đăng nhập thành công, khởi tạo các session
                    HttpContext.Session.SetString("admin_user_email", user.Email);
                    HttpContext.Session.SetString("admin_user_id", user.Id.ToString());

                    //di chuyển đeens các url
                    return Redirect("/Admin/Home");
                }
            }
            return Redirect("/Admin/Account/Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/Admin/Account/Login");
        }
    }
}
