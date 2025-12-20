using Domain.Entities;
using Newtonsoft.Json;

namespace Application.Dto;

public class AboutDto
{
  [JsonProperty("techStackId")]
  public string TechStackId { get; set; } = string.Empty;

  [JsonProperty("bio")]
  public BioDto Bio { get; set; } = new BioDto();

  [JsonProperty("funFact")]
  public string FunFact { get; set; } = string.Empty;
}

public class BioDto
{
  [JsonProperty("name")]
  public string Name { get; set; } = string.Empty;

  [JsonProperty("intro")]
  public string Intro { get; set; } = string.Empty;

  [JsonProperty("experience")]
  public string Experience { get; set; } = string.Empty;

  [JsonProperty("company")]
  public string Company { get; set; } = string.Empty;

  [JsonProperty("highlights")]
  public List<BioHighlightDto> Highlights { get; set; } = [];
}

public class BioHighlightDto
{
  [JsonProperty("icon")]
  public string Icon { get; set; } = string.Empty;

  [JsonProperty("text")]
  public string Text { get; set; } = string.Empty;

  [JsonProperty("highlight")]
  public string? Highlight { get; set; } = null;
}

public class FetchAboutDto
{
  [JsonProperty("id")]
  public string Id { get; set; } = string.Empty;

  [JsonProperty("bio")]
  public BioDto Bio { get; set; } = new BioDto();

  [JsonProperty("funFact")]
  public string FunFact { get; set; } = string.Empty;

  [JsonProperty("techStack")]
  public FetchTechStackDto TechStack { get; set; } = new FetchTechStackDto();
}

public class UpdateAboutDto
{
  [JsonProperty("bio")]
  public UpdateBioDto? Bio { get; set; } = new UpdateBioDto();

  [JsonProperty("funFact")]
  public string? FunFact { get; set; } = string.Empty;
}

public class UpdateBioDto
{
  [JsonProperty("name")]
  public string? Name { get; set; } = null;

  [JsonProperty("intro")]
  public string? Intro { get; set; } = null;

  [JsonProperty("experience")]
  public string? Experience { get; set; } = null;

  [JsonProperty("company")]
  public string? Company { get; set; } = null;

  [JsonProperty("highlights")]
  public List<UpdateBioHighlightDto>? Highlights { get; set; } = null;
}

public class UpdateBioHighlightDto
{
  [JsonProperty("icon")]
  public string? Icon { get; set; } = null;

  [JsonProperty("text")]
  public string? Text { get; set; } = null;

  [JsonProperty("highlight")]
  public string? Highlight { get; set; } = null;
}