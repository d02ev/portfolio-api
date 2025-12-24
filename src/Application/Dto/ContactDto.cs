using Newtonsoft.Json;

namespace Application.Dto;

public class ContactDto
{
  [JsonProperty("email")]
  public string Email { get; set; } = string.Empty;

  [JsonProperty("mobile")]
  public string Mobile { get; set; } = string.Empty;

  [JsonProperty("github")]
  public string Github { get; set; } = string.Empty;

  [JsonProperty("linkedin")]
  public string Linkedin { get; set; } = string.Empty;

  [JsonProperty("website")]
  public string Website { get; set; } = string.Empty;
}

public class FetchContactDto
{
  [JsonProperty("id")]
  public string Id { get; set; } = string.Empty;

  [JsonProperty("email")]
  public string Email { get; set; } = string.Empty;

  [JsonProperty("mobile")]
  public string Mobile { get; set; } = string.Empty;

  [JsonProperty("github")]
  public string Github { get; set; } = string.Empty;

  [JsonProperty("linkedin")]
  public string Linkedin { get; set; } = string.Empty;

  [JsonProperty("website")]
  public string Website { get; set; } = string.Empty;
}

public class UpdateContactDto
{
  [JsonProperty("email")]
  public string? Email { get; set; } = null;

  [JsonProperty("mobile")]
  public string? Mobile { get; set; } = null;

  [JsonProperty("github")]
  public string? Github { get; set; } = null;

  [JsonProperty("linkedin")]
  public string? Linkedin { get; set; } = null;

  [JsonProperty("website")]
  public string? Website { get; set; } = null;
}