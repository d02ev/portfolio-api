using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(ILogger<HealthController> logger) : ControllerBase
{
  private readonly ILogger<HealthController> _logger = logger;

  [HttpGet]
  public IActionResult GetHealth()
  {
    _logger.LogInformation("Started {Action}.", nameof(GetHealth));
    var result = new { statusCode = (int)HttpStatusCode.OK, message = "Server in Healthy.", datetime = DateTime.UtcNow };
    _logger.LogInformation("Completed {Action}.", nameof(GetHealth));
    return Ok(result);
  }
}
