using Newtonsoft.Json;

namespace Application.Dto;

public class ExperienceDto
{
  [JsonProperty("jobTitle")]
  public string JobTitle { get; set; } = string.Empty;

  [JsonProperty("companyName")]
  public string CompanyName { get; set; } = string.Empty;

  [JsonProperty("location")]
  public string Location { get; set; } = string.Empty;

  [JsonProperty("startDate")]
  public string StartDate { get; set; } = string.Empty;

  [JsonProperty("endDate")]
  public string? EndDate { get; set; } = null;

  [JsonProperty("description")]
  public List<string> Description { get; set; } = [];
}

public class FetchExperienceDto
{
  [JsonProperty("id")]
  public string Id { get; set; } = string.Empty;

  [JsonProperty("jobTitle")]
  public string JobTitle { get; set; } = string.Empty;

  [JsonProperty("companyName")]
  public string CompanyName { get; set; } = string.Empty;

  [JsonProperty("location")]
  public string Location { get; set; } = string.Empty;

  [JsonProperty("startDate")]
  public string StartDate { get; set; } = string.Empty;

  [JsonProperty("endDate")]
  public string? EndDate { get; set; } = null;

  [JsonProperty("description")]
  public List<string> Description { get; set; } = [];
}

public class UpdateExperienceDto
{
  [JsonProperty("jobTitle")]
  public string? JobTitle { get; set; } = null;

  [JsonProperty("companyName")]
  public string? CompanyName { get; set; } = null;

  [JsonProperty("location")]
  public string? Location { get; set; } = null;

  [JsonProperty("startDate")]
  public string? StartDate { get; set; } = null;

  [JsonProperty("endDate")]
  public string? EndDate { get; set; } = null;

  [JsonProperty("description")]
  public List<string>? Description { get; set; } = null;
}