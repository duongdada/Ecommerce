using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace E_Commerce.Controllers
{
    public class ProductsController : Controller
    {
        public MyDbContext db = new MyDbContext();

        // SỬA: Trang shop với filter Size/Color/Price
        public IActionResult Collection(int? page, string sortBy, string sizes, string colors, double? fromPrice, double? toPrice)
        {
            int page_number = page ?? 1;
            int page_size = 9;
            ViewBag.sortBy = sortBy ?? "newest";

            // Lấy tất cả sản phẩm
            List<ItemProduct> listRecord = db.Products.ToList();

            // Filter theo Size
            if (!string.IsNullOrEmpty(sizes))
            {
                var sizeList = sizes.Split(',').Select(s => s.Trim()).ToList();
                listRecord = listRecord.Where(p =>
                    !string.IsNullOrEmpty(p.AvailableSizes) &&
                    sizeList.Any(size => p.AvailableSizes.Contains(size))
                ).ToList();
            }

            // Filter theo Color
            if (!string.IsNullOrEmpty(colors))
            {
                var colorList = colors.Split(',').Select(c => c.Trim()).ToList();
                listRecord = listRecord.Where(p =>
                    !string.IsNullOrEmpty(p.AvailableColors) &&
                    colorList.Any(color => p.AvailableColors.Contains(color))
                ).ToList();
            }

            // Filter theo Price
            if (fromPrice.HasValue)
            {
                listRecord = listRecord.Where(p =>
                    (p.Price - (p.Price * p.Discount) / 100) >= fromPrice.Value
                ).ToList();
            }

            if (toPrice.HasValue)
            {
                listRecord = listRecord.Where(p =>
                    (p.Price - (p.Price * p.Discount) / 100) <= toPrice.Value
                ).ToList();
            }

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
                default:
                    listRecord = listRecord.OrderByDescending(item => item.Id).ToList();
                    break;
            }

            return View("ProductsCollection", listRecord.ToPagedList(page_number, page_size));
        }

        // các sản phẩm thuộc danh mục
        public IActionResult Category(int id, int? page)
        {
            int page_number = page ?? 1;
            int page_size = 9;
            ViewBag.CategoryId = id;

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
            ItemProduct record = db.Products.FirstOrDefault(item => item.Id == id);
            return View("ProductDetail", record);
        }

        // đánh giá số sao của sản phẩm
        public IActionResult Rate(int id)
        {
            int _Star = !String.IsNullOrEmpty(Request.Query["star"]) ? Convert.ToInt32(Request.Query["star"]) : 0;
            ItemRating record = new ItemRating();
            record.ProductId = id;
            record.Star = _Star;
            db.Rating.Add(record);
            db.SaveChanges();
            return Redirect("/Products/Detail/" + id);
        }
    }
}
