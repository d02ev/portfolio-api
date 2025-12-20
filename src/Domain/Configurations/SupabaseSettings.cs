namespace Domain.Configurations;

public class SupabaseSettings
{
  public string ProjectUrl { get; set; } = string.Empty;

  public string ServiceRoleKey { get; set; } = string.Empty;

  public string AnonKey { get; set; } = string.Empty;

  public string Bucket { get; set; } = string.Empty;
}