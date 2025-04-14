using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PhoneBookApi.DTOs;

namespace PhoneBookApi.Controllers
{
    public class AuthController : Controller
    {
        private readonly IMongoDatabase _database;

        public AuthController(IMongoDatabase database)
        {
            _database = database;
        }
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            return View();
        }
    }
}
