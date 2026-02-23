using Microsoft.AspNetCore.Mvc;
using E_Commerce.Areas.Admin.Attributes;

namespace E_Commerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    //Gọi Attribute theo cú pháp sau
    [CheckLogin]
    public class HomeController : Controller
    {
        [Area("Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
