using Microsoft.AspNetCore.Mvc;

namespace PhoneBookApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PhoneBookController : ControllerBase
{

    private readonly ILogger<PhoneBookController> _logger;

    public PhoneBookController(ILogger<PhoneBookController> logger)
    {
        _logger = logger;
    }
}
