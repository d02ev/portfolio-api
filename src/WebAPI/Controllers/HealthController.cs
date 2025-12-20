using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
  [HttpGet]
  public IActionResult GetHealth()
  {
    return Ok(new { statusCode = 200, responseCode = "HEALTH_CHECK", resourceName = "APP", message = "Server in Healthy." });
  }
}