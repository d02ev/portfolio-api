using System.Text.Json;
using Application.Dto;
using Application.Responses;
using Application.Services;
using Domain.Configurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService userService, IOptions<CookieSettings> options) : ControllerBase
{
  private readonly IUserService _userService = userService;
  private readonly CookieSettings _cookieSettings = options.Value;

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

    AddTokenCookie(result.Token.AccessToken);
    AddTokenCookie(result.Token.RefreshToken, "refresh");

    return Ok(result);
  }

  [HttpGet("token")]
  public async Task<IActionResult> GetShortLivedToken([FromQuery] string username)
  {
    var result = await _userService.GenerateShortLivedToken(username);
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
    Response.Cookies.Delete(_cookieSettings.RefreshTokenCookieName);
    Response.Cookies.Delete(_cookieSettings.AccessTokenCookieName);

    return Ok(new LogoutUserResponse());
  }

  private void AddTokenCookie(string token, string tokenType = "access")
  {
    var cookieName = _cookieSettings.AccessTokenCookieName;
    var cookieMaxAge = TimeSpan.FromMinutes(_cookieSettings.AccessTokenMaxAgeInMinutes);

    if (tokenType.Equals("refresh"))
    {
      cookieName = _cookieSettings.RefreshTokenCookieName;
      cookieMaxAge = TimeSpan.FromDays(_cookieSettings.RefreshTokenMaxAgeInDays);
    }

    Response.Cookies.Append(cookieName, token, new CookieOptions
    {
      MaxAge = cookieMaxAge,
      HttpOnly = _cookieSettings.HttpOnly,
      SameSite = SameSiteMode.None,
      Secure = !Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.ToLower().Equals("development"),
      Path = _cookieSettings.Path,
    });
  }
}