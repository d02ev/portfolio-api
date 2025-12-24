using Application.Dto;
using Application.Responses;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService userService) : ControllerBase
{
  private readonly IUserService _userService = userService;

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] UserDto userDto)
  {
    var result = await _userService.RegisterUser(userDto);
    return CreatedAtAction(nameof(Register), result);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] UserDto userDto)
  {
    var result = await _userService.LoginUser(userDto);

    return Ok(result);
  }

  [HttpPost("refresh/{token}")]
  public async Task<IActionResult> Refresh([FromRoute] string token)
  {
    var result = await _userService.RefreshAccessToken(token);
    return Ok(result);
  }

  [Authorize]
  [HttpPost("logout")]
  public IActionResult Logout()
  {
    return Ok(new LogoutUserResponse());
  }
}