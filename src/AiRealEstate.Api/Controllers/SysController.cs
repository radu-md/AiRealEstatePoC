using Microsoft.AspNetCore.Mvc;

namespace AiRealEstate.ApiAndWebApp.Controllers;

[ApiController, Route("api/[controller]")]
public class SysController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { ok = true, ts = DateTime.UtcNow });
}
