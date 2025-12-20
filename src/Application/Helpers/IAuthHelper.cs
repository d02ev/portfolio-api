using System.Security.Claims;

namespace Application.Helpers;

public interface IAuthHelper
{
  string GeneratePasswordHash(string plainPassword);

  bool ComparePassword(string plainPassword, string passwordHash);

  string GenerateAccessToken(IEnumerable<Claim> claims);

  string GenerateShortLivedAccessToken(IEnumerable<Claim> claims);

  string GenerateRefreshToken(IEnumerable<Claim> claims);

  ClaimsPrincipal? DecodeToken(string token, string tokenType = "access");
}