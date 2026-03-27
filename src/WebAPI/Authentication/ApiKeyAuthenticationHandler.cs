using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Domain.Configurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace WebAPI.Authentication;

public class ApiKeyAuthenticationHandler(
  IOptionsMonitor<AuthenticationSchemeOptions> options,
  ILoggerFactory logger,
  UrlEncoder encoder,
  IOptions<ApiKeySettings> apiKeyOptions
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
  private readonly ApiKeySettings _apiKeySettings = apiKeyOptions.Value;

  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    var headerName = string.IsNullOrWhiteSpace(_apiKeySettings.HeaderName)
      ? "X-Api-Key"
      : _apiKeySettings.HeaderName;

    if (string.IsNullOrWhiteSpace(_apiKeySettings.Key))
    {
      Logger.LogError("API key authentication configuration is missing.");
      return Task.FromResult(AuthenticateResult.Fail("API key authentication is not configured."));
    }

    if (!Request.Headers.TryGetValue(headerName, out var headerValues))
    {
      Logger.LogDebug("API key header not found. Falling back to other authentication schemes.");
      return Task.FromResult(AuthenticateResult.NoResult());
    }

    var providedApiKey = headerValues.ToString();
    if (string.IsNullOrWhiteSpace(providedApiKey))
    {
      Logger.LogWarning("API key header provided but empty.");
      return Task.FromResult(AuthenticateResult.Fail("API key is missing."));
    }

    if (!KeysMatch(providedApiKey, _apiKeySettings.Key))
    {
      Logger.LogWarning("API key authentication failed due to invalid key.");
      return Task.FromResult(AuthenticateResult.Fail("API key is invalid."));
    }

    var claims = new[]
    {
      new Claim(ClaimTypes.NameIdentifier, "cli"),
      new Claim(ClaimTypes.Name, "cli-client"),
      new Claim("auth_method", "api_key"),
    };
    var identity = new ClaimsIdentity(claims, Scheme.Name);
    var principal = new ClaimsPrincipal(identity);
    var ticket = new AuthenticationTicket(principal, Scheme.Name);

    Logger.LogInformation("API key authentication succeeded.");
    return Task.FromResult(AuthenticateResult.Success(ticket));
  }

  private static bool KeysMatch(string providedApiKey, string expectedApiKey)
  {
    var providedBytes = Encoding.UTF8.GetBytes(providedApiKey);
    var expectedBytes = Encoding.UTF8.GetBytes(expectedApiKey);

    return providedBytes.Length == expectedBytes.Length
      && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
  }
}
