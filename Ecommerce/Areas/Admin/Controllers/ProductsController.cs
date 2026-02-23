using E_Commerce.Areas.Admin.Attributes;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
//sử dụng thư viện sau để phân trang
using X.PagedList;

namespace project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CheckLogin]
    public class ProductsController : Controller
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
            int page_size = 4;

            ViewBag.Page = page_number;

            //lấy danh sách các bản ghi
            List<ItemProduct> listRecord = db.Products.OrderByDescending(item => item.Id).ToList();
            return View("Read", listRecord.ToPagedList(page_number, page_size));
        }
        //update
        public IActionResult Update(int id, int page=1)
        {
            //lấy 1 bản ghi tương ứng với id truyền vào
            ItemProduct row = db.Products.FirstOrDefault(item => item.Id == id);
            //Tạo biến formAction để lưu action của form
            ViewBag.formAction = $"/Admin/Products/UpdatePost/{id}?page={page}";
            ViewBag.CurrentPage = page;
            return View("CreateUpdate", row);
        }
        //update Post
        [HttpPost]
        public IActionResult UpdatePost(int id, int page, IFormCollection fc)
        {
            string _Name = fc["Name"];
            string _Description = fc["Description"];
            string _Content = fc["Content"];
            double _Price = Convert.ToDouble(fc["Price"]);
            double _Discount = Convert.ToDouble(fc["Discount"]);
            int _Hot = !string.IsNullOrEmpty(fc["Hot"]) ? 1 : 0;

            ItemProduct row = db.Products.FirstOrDefault(item => item.Id == id);
            if (row != null)
            {
                row.Name = _Name;
                row.Description = _Description;
                row.Content = _Content;
                row.Price = _Price;
                row.Discount = _Discount;
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
                                "wwwroot/Upload/Products",
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
                            "wwwroot/Upload/Products",
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

            CreateUpdateCategoriesProducts(id);
            return RedirectToAction("Read", new { page = page });
        }

        public void CreateUpdateCategoriesProducts(int _ProductId)
        {
            //lấy giá trị của biến form có name=Categories
            List<string> categories = Request.Form["Categories"].ToList();
            //xóa hết các bản ghi tương ứng với _ProductId
            List<ItemCategoryProduct> list_categories_products = db.CategoriesProducts.Where(item => item.ProductId == _ProductId).ToList();
            foreach (var item in list_categories_products)
            {
                db.CategoriesProducts.Remove(item);
                db.SaveChanges();
            }
            //---
            foreach (string category in categories)
            {
                int _CategoryId = Convert.ToInt32(category);
                //thêm mới bản ghi vào table CategoriesProducts
                ItemCategoryProduct record = new ItemCategoryProduct();
                record.ProductId = _ProductId;
                record.CategoryId = _CategoryId;
                db.CategoriesProducts.Add(record);
                db.SaveChanges();
            }
        }
        //Create
        public IActionResult Create()
        {
            ViewBag.formAction = "/Admin/Products/CreatePost";
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
            ItemProduct row = new ItemProduct();
            row.Name = _Name;
            row.Description = _Description;
            row.Content = _Content;
            row.Price = _Price;
            row.Discount = _Discount;
            row.Hot = _Hot;
            //---
            //try
            {
                //updaload ảnh
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files[0];

                    if (file != null && file.Length > 0)
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string path = Path.Combine("wwwroot/Upload/Products", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        row.Photo = fileName;
                    }
                }
                //---
                db.Update(row);
                db.SaveChanges();
            }
            //catch {; }
            //gọi hàm để update Categories
            CreateUpdateCategoriesProducts(row.Id);
            return Redirect("/Admin/Products");
        }
        //Delete
        public IActionResult Delete(int id)
        {
            ItemProduct row = db.Products.FirstOrDefault(item => item.Id == id);
            if (row != null)
            {
                // 1. Xóa ảnh vật lý (nếu có)
                if (!string.IsNullOrEmpty(row.Photo))
                {
                    string photoPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/Upload/Products",
                        row.Photo
                    );

                    if (System.IO.File.Exists(photoPath))
                    {
                        System.IO.File.Delete(photoPath);
                    }
                }

                // 2. Xóa bản ghi trong database
                db.Products.Remove(row);
                db.SaveChanges();
            }

            return Redirect("/Admin/Products");
        }

    }
}
