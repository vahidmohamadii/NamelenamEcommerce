using Microsoft.AspNetCore.Mvc;

namespace NaderEcommerce.WebApi.Controllers;

[ApiController]
[Route("api/status")]
public sealed class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            name = "NaderEcommerce API",
            status = "Running",
            utcTime = DateTimeOffset.UtcNow
        });
    }
}
