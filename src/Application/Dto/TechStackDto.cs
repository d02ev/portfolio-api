using Newtonsoft.Json;

namespace Application.Dto;

public class TechStackDto
{
  [JsonProperty("languages")]
  public List<string> Languages { get; set; } = [];

  [JsonProperty("frameworksAndPlatforms")]
  public List<string> FrameworksAndPlatforms { get; set; } = [];

  [JsonProperty("databases")]
  public List<string> Databases { get; set; } = [];

  [JsonProperty("cloudAndDevOps")]
  public List<string> CloudAndDevOps { get; set; } = [];

  [JsonProperty("others")]
  public List<string> Others { get; set; } = [];
}

public class FetchTechStackDto
{
  [JsonProperty("id")]
  public string Id { get; set; } = string.Empty;

  [JsonProperty("languages")]
  public List<string> Languages { get; set; } = [];

  [JsonProperty("frameworksAndPlatforms")]
  public List<string> FrameworksAndPlatforms { get; set; } = [];

  [JsonProperty("databases")]
  public List<string> Databases { get; set; } = [];

  [JsonProperty("cloudAndDevOps")]
  public List<string> CloudAndDevOps { get; set; } = [];

  [JsonProperty("others")]
  public List<string> Others { get; set; } = [];
}

public class UpdateTechStackDto
{
  [JsonProperty("languages")]
  public List<string>? Languages { get; set; } = null;

  [JsonProperty("frameworksAndPlatforms")]
  public List<string>? FrameworksAndPlatforms { get; set; } = null;

  [JsonProperty("databases")]
  public List<string>? Databases { get; set; } = null;

  [JsonProperty("cloudAndDevOps")]
  public List<string>? CloudAndDevOps { get; set; } = null;

  [JsonProperty("others")]
  public List<string>? Others { get; set; } = null;
}
