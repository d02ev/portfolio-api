namespace Domain.Configurations;

public class JwtSettings
{
  public string AccessTokenSecretKey { get; set; } = string.Empty;

  public string RefreshTokenSecretKey { get; set; } = string.Empty;

  public int AccessTokenExpiryInMinutes { get; set; } = 60;

  public int ShortLivedAccessTokenExpiryInMinutes { get; set; } = 5;

  public int RefreshTokenExpiryInDays { get; set; } = 30;
}