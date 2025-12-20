using Newtonsoft.Json;

namespace Application.Dto;

public class TechStackDto
{
  [JsonProperty("languages")]
  public List<string> Languages { get; set; } = [];

  [JsonProperty("frameworks")]
  public List<string> Frameworks { get; set; } = [];

  [JsonProperty("databases")]
  public List<string> Databases { get; set; } = [];

  [JsonProperty("tools")]
  public List<string> Tools { get; set; } = [];

  [JsonProperty("cloud")]
  public List<string> Cloud { get; set; } = [];

  [JsonProperty("ai")]
  public List<string> Ai { get; set; } = [];
}

public class FetchTechStackDto
{
  [JsonProperty("id")]
  public string Id { get; set; } = string.Empty;

  [JsonProperty("languages")]
  public List<string> Languages { get; set; } = [];

  [JsonProperty("frameworks")]
  public List<string> Frameworks { get; set; } = [];

  [JsonProperty("databases")]
  public List<string> Databases { get; set; } = [];

  [JsonProperty("tools")]
  public List<string> Tools { get; set; } = [];

  [JsonProperty("cloud")]
  public List<string> Cloud { get; set; } = [];

  [JsonProperty("ai")]
  public List<string> Ai { get; set; } = [];
}

public class UpdateTechStackDto
{
  [JsonProperty("languages")]
  public List<string>? Languages { get; set; } = null;

  [JsonProperty("frameworks")]
  public List<string>? Frameworks { get; set; } = null;

  [JsonProperty("databases")]
  public List<string>? Databases { get; set; } = null;

  [JsonProperty("tools")]
  public List<string>? Tools { get; set; } = null;

  [JsonProperty("cloud")]
  public List<string>? Cloud { get; set; } = null;

  [JsonProperty("ai")]
  public List<string>? Ai { get; set; } = null;
}
