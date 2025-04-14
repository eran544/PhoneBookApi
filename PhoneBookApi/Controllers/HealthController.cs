using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace PhoneBookApi.Controllers
{
    public class HealthController : Controller
    {
        private readonly IMongoDatabase _database;

        public HealthController(IMongoDatabase database)
        {
            _database = database;
        }

        [HttpGet("check-mongo")]
        public async Task<IActionResult> CheckMongo()
        {
            try
            {
                // Mongo has no "ping", so try listing collections
                var collections = await _database.ListCollectionNames().ToListAsync();
                return Ok(new { connected = true,  collections });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { connected = false, error = ex.Message });
            }
        }
    }
}
