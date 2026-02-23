using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
//sử dụng thư viện sau để phân trang

namespace E_Commerce.Controllers
{
    public class ProductsController : Controller
    {
        public MyDbContext db = new MyDbContext();

        // THÊM MỚI: Trang shop - hiển thị tất cả sản phẩm
        public IActionResult Collection(int? page, string sortBy)
        {
            // xác định số trang hiện tại
            int page_number = page ?? 1;
            // số bản ghi trên một trang
            int page_size = 9;

            ViewBag.sortBy = sortBy ?? "newest";

            // Lấy tất cả sản phẩm
            List<ItemProduct> listRecord = db.Products.ToList();

            // Sắp xếp
            switch (sortBy)
            {
                case "price-asc":
                    listRecord = listRecord.OrderBy(item => item.Price - (item.Price * item.Discount) / 100).ToList();
                    break;
                case "price-desc":
                    listRecord = listRecord.OrderByDescending(item => item.Price - (item.Price * item.Discount) / 100).ToList();
                    break;
                case "name-asc":
                    listRecord = listRecord.OrderBy(item => item.Name).ToList();
                    break;
                case "name-desc":
                    listRecord = listRecord.OrderByDescending(item => item.Name).ToList();
                    break;
                default: // newest
                    listRecord = listRecord.OrderByDescending(item => item.Id).ToList();
                    break;
            }

            return View("ProductsCollection", listRecord.ToPagedList(page_number, page_size));
        }

        // các sản phẩm thuộc danh mục
        public IActionResult Category(int id, int? page)
        {
            // xác định số trang hiện tại
            int page_number = page ?? 1;
            // số bản ghi trên một trang
            int page_size = 9;
            ViewBag.CategoryId = id;
            // lấy danh sách các bản ghi
            List<ItemProduct> listRecord = (from product in db.Products
                                            join category_product in db.CategoriesProducts
                                                on product.Id equals category_product.ProductId
                                            join category in db.Categories
                                                on category_product.CategoryId equals category.Id
                                            where category_product.CategoryId == id
                                            select product).ToList();
            return View("ProductsCategory", listRecord.ToPagedList(page_number, page_size));
        }

        // chi tiết sản phẩm
        public IActionResult Detail(int id)
        {
            // lấy một bản ghi
            ItemProduct record = db.Products.FirstOrDefault(item => item.Id == id);
            return View("ProductDetail", record);
        }

        // đánh giá số sao của sản phẩm
        public IActionResult Rate(int id)
        {
            // lấy biến star truyền từ url
            int _Star = !String.IsNullOrEmpty(Request.Query["star"]) ? Convert.ToInt32(Request.Query["star"]) : 0;
            // thêm bản ghi vào table Rating
            ItemRating record = new ItemRating();
            record.ProductId = id;
            record.Star = _Star;
            db.Rating.Add(record);
            db.SaveChanges();
            return Redirect("/Products/Detail/" + id);
        }
    }
}