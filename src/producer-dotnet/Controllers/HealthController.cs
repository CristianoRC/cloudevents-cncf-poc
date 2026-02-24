using Microsoft.AspNetCore.Mvc;

namespace ProducerDotnet.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", service = "producer-dotnet" });
    }
}
