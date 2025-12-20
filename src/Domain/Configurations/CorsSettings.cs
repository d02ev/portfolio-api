namespace Domain.Configurations;

public class CorsSettings
{
  public string Hosts { get; set; } = string.Empty;

  public List<string> Methods { get; set; } = [];
}