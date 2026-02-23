using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
