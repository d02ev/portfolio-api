using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Helpers;
using Domain.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Helpers;

public class AuthHelper(IOptions<JwtSettings> options) : IAuthHelper
{
  private readonly JwtSettings _jwtSettings = options.Value;

  public bool ComparePassword(string plainPassword, string passwordHash)
  {
    return BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
  }

  public ClaimsPrincipal? DecodeToken(string token, string tokenType = "access")
  {
    var key = tokenType.Equals("refresh")
      ? Encoding.UTF8.GetBytes(_jwtSettings.RefreshTokenSecretKey)
      : Encoding.UTF8.GetBytes(_jwtSettings.AccessTokenSecretKey);
    var tokenHandler = new JwtSecurityTokenHandler();

    try
    {
      return tokenHandler.ValidateToken(token, new TokenValidationParameters
      {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        LifetimeValidator = (notBefore, expires, securityToken, validationParameters) =>
        {
          if (expires != null)
          {
            return expires > DateTime.UtcNow;
          }

          return false;
        },
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero,
      }, out _);
    }
    catch
    {
      return null;
    }
  }

  public string GenerateAccessToken(IEnumerable<Claim> claims)
  {
    var key = Encoding.UTF8.GetBytes(_jwtSettings.AccessTokenSecretKey);
    var token = new JwtSecurityToken
    (
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryInMinutes),
      signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public string GeneratePasswordHash(string plainPassword)
  {
    return BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 10);
  }

  public string GenerateRefreshToken(IEnumerable<Claim> claims)
  {
    var key = Encoding.UTF8.GetBytes(_jwtSettings.RefreshTokenSecretKey);
    var token = new JwtSecurityToken
    (
      claims: claims,
      expires: DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays),
      signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public string GenerateShortLivedAccessToken(IEnumerable<Claim> claims)
  {
    var key = Encoding.UTF8.GetBytes(_jwtSettings.AccessTokenSecretKey);
    var token = new JwtSecurityToken
    (
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ShortLivedAccessTokenExpiryInMinutes),
      signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}