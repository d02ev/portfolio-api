namespace Domain.Configurations;

public class CookieSettings
{
  public bool HttpOnly { get; set; }

  public string Path { get; set; } = string.Empty;

  public string AccessTokenCookieName { get; set; } = string.Empty;

  public string RefreshTokenCookieName { get; set; } = string.Empty;

  public int RefreshTokenMaxAgeInDays { get; set; } = 30;

  public int AccessTokenMaxAgeInMinutes { get; set; } = 60;
}