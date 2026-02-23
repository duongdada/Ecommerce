using E_Commerce.Areas.Admin.Attributes;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;

//sử dụng thư viện sau để phân trang
using X.PagedList;

using BC = BCrypt.Net;

namespace project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CheckLogin]
    public class NewsController : Controller
    {
        public MyDbContext db = new MyDbContext();
        public IActionResult Index()
        {
            return RedirectToAction("Read");
        }
        //hiển thị danh sách các bản ghi
        public IActionResult Read(int? page)
        {
            //xác định số trang hiện tại
            int page_number = page ?? 1;
            //số bản ghi trên một trang
            int page_size = 6;

            ViewBag.Page = page_number;

            //lấy danh sách các bản ghi
            List<ItemNews> listRecord = db.News.OrderByDescending(item => item.Id).ToList();
            return View("Read", listRecord.ToPagedList(page_number, page_size));
        }
        //update
        public IActionResult Update(int id, int page =1)
        {
            //lấy 1 bản ghi tương ứng với id truyền vào
            ItemNews row = db.News.FirstOrDefault(item => item.Id == id);
            //Tạo biến formAction để lưu action của form
            ViewBag.formAction = $"/Admin/News/UpdatePost/{id}?page={page}";
            return View("CreateUpdate", row);
        }
        //update Post
        [HttpPost]
        public IActionResult UpdatePost(int id,int page, IFormCollection fc)
        {
            string _Name = fc["Name"];
            string _Description = fc["Description"];
            string _Content = fc["Content"];
            int _Hot = !string.IsNullOrEmpty(fc["Hot"]) ? 1 : 0;

            ItemNews row = db.News.FirstOrDefault(item => item.Id == id);
            if (row != null)
            {
                row.Name = _Name;
                row.Description = _Description;
                row.Content = _Content;
                row.Hot = _Hot;

                // ===== XỬ LÝ ẢNH =====
                if (Request.Form.Files != null && Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files[0];

                    if (file != null && file.Length > 0)
                    {
                        // 1. Xóa ảnh cũ
                        if (!string.IsNullOrEmpty(row.Photo))
                        {
                            string oldPath = Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot/Upload/News",
                                row.Photo
                            );

                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // 2. Upload ảnh mới
                        string newPhoto = Path.GetFileName(file.FileName);
                        string newPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot/Upload/News",
                            newPhoto
                        );

                        using (var stream = new FileStream(newPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        // 3. Cập nhật tên ảnh mới vào DB
                        row.Photo = newPhoto;
                    }
                }

                db.Update(row);
                db.SaveChanges();
            }

            return RedirectToAction("Read", new { page = page });
        }


        //Create
        public IActionResult Create()
        {
            ViewBag.formAction = "/Admin/News/CreatePost";
            return View("CreateUpdate");
        }
        //update Post
        [HttpPost]
        public IActionResult CreatePost(IFormCollection fc)
        {
            string _Name = fc["Name"];
            string _Description = fc["Description"];
            string _Content = fc["Content"];
            double _Price = Convert.ToDouble(fc["Price"]);
            double _Discount = Convert.ToDouble(fc["Discount"]);
            int _Hot = !String.IsNullOrEmpty(fc["Hot"]) ? 1 : 0;
            //update bản ghi
            ItemNews row = new ItemNews();
            row.Name = _Name;
            row.Description = _Description;
            row.Content = _Content;
            row.Hot = _Hot;
            //---
            try
            {
                //updaload ảnh
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files[0];

                    if (file != null && file.Length > 0)
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string path = Path.Combine("wwwroot/Upload/News", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        row.Photo = fileName;
                    }
                }
                //---
                db.Add(row);
                db.SaveChanges();
            }
            catch {; }
            //gọi hàm để update Categories
            return Redirect("/Admin/News");
        }
        //Delete
        public IActionResult Delete(int id)
        {
            ItemNews row = db.News.FirstOrDefault(item => item.Id == id);
            if (row != null)
            {
                // 1. Xóa ảnh vật lý (nếu có)
                if (!string.IsNullOrEmpty(row.Photo))
                {
                    string photoPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/Upload/News",
                        row.Photo
                    );

                    if (System.IO.File.Exists(photoPath))
                    {
                        System.IO.File.Delete(photoPath);
                    }
                }

                // 2. Xóa bản ghi trong database
                db.News.Remove(row);
                db.SaveChanges();
            }

            return Redirect("/Admin/News");
        }

    }
}
