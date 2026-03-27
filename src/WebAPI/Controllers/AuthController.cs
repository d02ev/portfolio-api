using Application.Dto;
using Application.Responses;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService userService, ILogger<AuthController> logger) : ControllerBase
{
  private readonly IUserService _userService = userService;
  private readonly ILogger<AuthController> _logger = logger;

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] UserDto userDto)
  {
    _logger.LogInformation("Started {Action}.", nameof(Register));
    var result = await _userService.RegisterUser(userDto);
    _logger.LogInformation("Completed {Action}.", nameof(Register));
    return CreatedAtAction(nameof(Register), result);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] UserDto userDto)
  {
    _logger.LogInformation("Started {Action}.", nameof(Login));
    var result = await _userService.LoginUser(userDto);
    _logger.LogInformation("Completed {Action}.", nameof(Login));
    return Ok(result);
  }

  [HttpPost("refresh/{token}")]
  public async Task<IActionResult> Refresh([FromRoute] string token)
  {
    _logger.LogInformation("Started {Action}.", nameof(Refresh));
    var result = await _userService.RefreshAccessToken(token);
    _logger.LogInformation("Completed {Action}.", nameof(Refresh));
    return Ok(result);
  }

  [Authorize]
  [HttpPost("logout")]
  public IActionResult Logout()
  {
    _logger.LogInformation("Started {Action}.", nameof(Logout));
    _logger.LogInformation("Completed {Action}.", nameof(Logout));
    return Ok(new LogoutUserResponse());
  }
}
