using Newtonsoft.Json;

namespace Application.Dto;

public class EducationDto
{
  [JsonProperty("institute")]
  public string Institute { get; set; } = string.Empty;

  [JsonProperty("startDate")]
  public string StartDate { get; set; } = string.Empty;

  [JsonProperty("endDate")]
  public string EndDate { get; set; } = string.Empty;

  [JsonProperty("degree")]
  public string Degree { get; set; } = string.Empty;

  [JsonProperty("grade")]
  public string Grade { get; set; } = string.Empty;

  [JsonProperty("coursework")]
  public List<string> Coursework { get; set; } = [];
}

public class FetchEducationDto
{
  [JsonProperty("id")]
  public string Id { get; set; } = string.Empty;

  [JsonProperty("institute")]
  public string Institute { get; set; } = string.Empty;

  [JsonProperty("startDate")]
  public string StartDate { get; set; } = string.Empty;

  [JsonProperty("endDate")]
  public string EndDate { get; set; } = string.Empty;

  [JsonProperty("degree")]
  public string Degree { get; set; } = string.Empty;

  [JsonProperty("grade")]
  public string Grade { get; set; } = string.Empty;

  [JsonProperty("coursework")]
  public List<string> Coursework { get; set; } = [];
}

public class UpdateEducationDto
{
  [JsonProperty("institute")]
  public string? Institute { get; set; } = null;

  [JsonProperty("startDate")]
  public string? StartDate { get; set; } = null;

  [JsonProperty("endDate")]
  public string? EndDate { get; set; } = null;

  [JsonProperty("degree")]
  public string? Degree { get; set; } = null;

  [JsonProperty("grade")]
  public string? Grade { get; set; } = null;

  [JsonProperty("coursework")]
  public List<string>? Coursework { get; set; } = null;
}