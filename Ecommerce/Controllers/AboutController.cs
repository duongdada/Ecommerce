using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
