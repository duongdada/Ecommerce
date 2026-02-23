using Microsoft.AspNetCore.Mvc;
using E_Commerce.Models;
using BC = BCrypt.Net;
using E_Commerce.Areas.Admin.Attributes;

//sử dụng thư viện sau để phân trang
using X.PagedList;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Identity;


namespace E_Commerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CheckLogin]
    public class UsersController : Controller
    {
        public MyDbContext db = new MyDbContext();
        public IActionResult Index()
        {
            return RedirectToAction("Read");
        }
        
        //Hiển thị danh sách các bản ghi
        public IActionResult Read(int? page)
        {
            //Xác định số trang hiện tại
            int page_number = page ?? 1;
            //Số bản ghi trên 1 trang
            int page_size = 4;

            ViewBag.Page = page_number;
            //Lấy danh sách các bản ghi
            List<RowUser> listUser = db.Users.OrderByDescending(c => c.Id).ToList();
            return View("Read",listUser.ToPagedList(page_number,page_size));
        }
        public IActionResult Update(int id, int page =1)
        {
            //Lấy 1 bản ghi tương ứng với id truyền vào
            RowUser rowUser = db.Users.FirstOrDefault(c=>c.Id == id);
            //Tạo biến formAction để lưu Action của form
            ViewBag.formAction = $"/Admin/Users/UpdatePost/{id}?page={page}";
            return View("CreateUpdate",rowUser);
        }
        [HttpPost]
        public IActionResult UpdatePost(int id, int page, IFormCollection fc)
        {
            //Cách 1: Lấy giá trị của form control theo IFormCollection
            string name = fc["name"];
            //Cách 2: Lấy giá trị của form control theo đối tượng Request
            string email = Request.Form["email"];
            string password = Request.Form["password"];
            //Update bản ghi
            RowUser row = db.Users.FirstOrDefault(c=>c.Id==id);
            if (row != null)
            {
                row.Name = name;
                row.Email = email;
                //Nếu password không rống thì update password
                if (!String.IsNullOrEmpty(password))
                {
                    row.Password = BC.BCrypt.HashPassword(password);
                }
                db.Update(row);
                db.SaveChanges();
            }
            return RedirectToAction("Read", new { page = page });
        }

        //Create
        public IActionResult Create()
        {
            ViewBag.formAction = "/Admin/Users/CreatePost";
            return View("CreateUpdate");
        }
        //CreatePost
        [HttpPost]
        public IActionResult CreatePost(IFormCollection fc)
        {
            //Cách 1: Lấy giá trị của form control theo IFormCollection
            string name = fc["name"];
            //Cách 2: lấy giá trị của form control theo đối tượng Request
            string email = Request.Form["email"];
            string password = Request.Form["password"];
            //Nghiên cưu thêm: kiểm tra email đã tồn tại hay chưa, nếu chưa tồn tại thì mới thêm
            //tạo bản ghi để insert
            RowUser row = new RowUser();
            row.Name = name;
            row.Email = email;
            row.Password = BC.BCrypt.HashPassword(password);
            //insert vào csdl
            db.Users.Add(row);
            db.SaveChanges();
            return Redirect("/Admin/Users");
        }

        //Delete
        public IActionResult Delete(int id)
        {
            //Lấy 1 bản ghi tương ứng với id truyền vào
            RowUser rowUser = db.Users.FirstOrDefault(c => c.Id == id);
            if (rowUser != null)
            {
                db.Users.Remove(rowUser);
                db.SaveChanges();
            }
            return Redirect("/Admin/Users");
        }
    }
}
