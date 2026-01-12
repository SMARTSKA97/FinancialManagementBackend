using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        // Correctly read the full informational version string, including the suffix.
        var informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        return Ok(new { version = informationalVersion ?? "N/A" });
    }
}
