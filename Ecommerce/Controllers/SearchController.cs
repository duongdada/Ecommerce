using Microsoft.AspNetCore.Mvc;
using E_Commerce.Models;
using X.PagedList;


namespace E_Commerce.Controllers
{
    public class SearchController : Controller
    {
        public MyDbContext db = new MyDbContext();

        // Tìm kiếm theo giá (Price Range Slider)
        public IActionResult SearchPrice(int? page, double? fromPrice, double? toPrice, string sortBy)
        {
            int page_number = page ?? 1;
            int page_size = 8;

            // Lưu giá trị người dùng nhập vào ViewBag để hiển thị lại
            ViewBag.fromPrice = fromPrice ?? 0;
            ViewBag.toPrice = toPrice ?? 0;
            ViewBag.sortBy = sortBy ?? "newest";

            List<ItemProduct> listRecord = db.Products.ToList();

            // Trường hợp 1: Chỉ nhập "Giá từ" -> Lọc từ giá đó trở lên (đến giá cao nhất)
            if (fromPrice.HasValue && fromPrice > 0 && (!toPrice.HasValue || toPrice == 0))
            {
                listRecord = listRecord.Where(item =>
                    (item.Price - (item.Price * item.Discount) / 100) >= fromPrice.Value
                ).ToList();
            }
            // Trường hợp 2: Chỉ nhập "Giá đến" -> Lọc từ 0 đến giá đó
            else if (toPrice.HasValue && toPrice > 0 && (!fromPrice.HasValue || fromPrice == 0))
            {
                listRecord = listRecord.Where(item =>
                    (item.Price - (item.Price * item.Discount) / 100) <= toPrice.Value
                ).ToList();
            }
            // Trường hợp 3: Nhập cả 2 -> Lọc trong khoảng từ giá 1 đến giá 2
            else if (fromPrice.HasValue && fromPrice > 0 && toPrice.HasValue && toPrice > 0)
            {
                listRecord = listRecord.Where(item =>
                {
                    var finalPrice = item.Price - (item.Price * item.Discount) / 100;
                    return finalPrice >= fromPrice.Value && finalPrice <= toPrice.Value;
                }).ToList();
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
                default: // newest
                    listRecord = listRecord.OrderByDescending(item => item.Id).ToList();
                    break;
            }

            return View("SearchPrice", listRecord.ToPagedList(page_number, page_size));
        }

        // Tìm kiếm theo tên (Search Bar)
        public IActionResult SearchName(int? page, string keyword, string sortBy)
        {
            int page_number = page ?? 1;
            int page_size = 8;

            ViewBag.keyword = keyword ?? "";
            ViewBag.sortBy = sortBy ?? "newest";

            List<ItemProduct> listRecord = db.Products.ToList();

            if (!String.IsNullOrEmpty(keyword))
            {
                listRecord = listRecord.Where(item =>
                    item.Name.ToLower().Contains(keyword.ToLower())
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
                default: // newest
                    listRecord = listRecord.OrderByDescending(item => item.Id).ToList();
                    break;
            }

            return View("SearchName", listRecord.ToPagedList(page_number, page_size));
        }

        // Tìm kiếm nâng cao (Filter tổng hợp)
        public IActionResult Filter(int? page, string keyword, double? fromPrice, double? toPrice,
                                    int? categoryId, string sortBy)
        {
            int page_number = page ?? 1;
            int page_size = 8;

            ViewBag.keyword = keyword ?? "";
            ViewBag.fromPrice = fromPrice ?? 0;
            ViewBag.toPrice = toPrice ?? 0;
            ViewBag.categoryId = categoryId ?? 0;
            ViewBag.sortBy = sortBy ?? "newest";

            List<ItemProduct> listRecord = db.Products.ToList();

            // Lọc theo keyword
            if (!String.IsNullOrEmpty(keyword))
            {
                listRecord = listRecord.Where(item =>
                    item.Name.ToLower().Contains(keyword.ToLower())
                ).ToList();
            }

            // Lọc theo category
            if (categoryId.HasValue && categoryId > 0)
            {
                var productIds = db.CategoriesProducts
                    .Where(cp => cp.CategoryId == categoryId.Value)
                    .Select(cp => cp.ProductId)
                    .ToList();

                listRecord = listRecord.Where(p => productIds.Contains(p.Id)).ToList();
            }

            // Lọc theo giá - Áp dụng cùng logic với SearchPrice
            if (fromPrice.HasValue && fromPrice > 0 && (!toPrice.HasValue || toPrice == 0))
            {
                listRecord = listRecord.Where(item =>
                    (item.Price - (item.Price * item.Discount) / 100) >= fromPrice.Value
                ).ToList();
            }
            else if (toPrice.HasValue && toPrice > 0 && (!fromPrice.HasValue || fromPrice == 0))
            {
                listRecord = listRecord.Where(item =>
                    (item.Price - (item.Price * item.Discount) / 100) <= toPrice.Value
                ).ToList();
            }
            else if (fromPrice.HasValue && fromPrice > 0 && toPrice.HasValue && toPrice > 0)
            {
                listRecord = listRecord.Where(item =>
                {
                    var finalPrice = item.Price - (item.Price * item.Discount) / 100;
                    return finalPrice >= fromPrice.Value && finalPrice <= toPrice.Value;
                }).ToList();
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
                default: // newest
                    listRecord = listRecord.OrderByDescending(item => item.Id).ToList();
                    break;
            }

            return View("Filter", listRecord.ToPagedList(page_number, page_size));
        }
    }
}