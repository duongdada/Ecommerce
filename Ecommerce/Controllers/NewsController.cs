using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
//sử dụng thư viện sau để phân trang
using X.PagedList;

namespace project.Controllers
{
    public class NewsController : Controller
    {
        public MyDbContext db = new MyDbContext();
        //hiển thị danh sách các bản ghi
        public IActionResult Index(int? page)
        {
            //xác định số trang hiện tại
            int page_number = page ?? 1;
            //số bản ghi trên một trang
            int page_size = 6;
            //lấy danh sách các bản ghi
            List<ItemNews> listRecord = db.News.OrderByDescending(item => item.Id).ToList();
            return View("Index", listRecord.ToPagedList(page_number, page_size));
        }
        public IActionResult Detail(int id)
        {
            //lấy một bản ghi
            ItemNews record = db.News.FirstOrDefault(item => item.Id == id);
            return View("Detail", record);
        }
    }
}
