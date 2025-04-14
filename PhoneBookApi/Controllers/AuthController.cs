using Microsoft.AspNetCore.Mvc;

namespace PhoneBookApi.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
