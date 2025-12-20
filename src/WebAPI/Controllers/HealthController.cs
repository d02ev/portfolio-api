using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
  [HttpGet]
  public IActionResult GetHealth()
  {
    return Ok(new { statusCode = (int)HttpStatusCode.OK, message = "Server in Healthy.", datetime = DateTime.UtcNow });
  }
}