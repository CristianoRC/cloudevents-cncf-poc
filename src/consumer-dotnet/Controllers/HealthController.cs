using Microsoft.AspNetCore.Mvc;

namespace ConsumerDotnet.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", service = "consumer-dotnet" });
    }
}
