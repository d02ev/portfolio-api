using Newtonsoft.Json;

namespace Application.Dto;

public class ProjectDto
{
  [JsonProperty("displayName")]
  public string DisplayName { get; set; } = string.Empty;

  [JsonProperty("shortDescription")]
  public string ShortDescription { get; set; } = string.Empty;

  [JsonProperty("longDescription")]
  public string? LongDescription { get; set; } = string.Empty;

  [JsonProperty("techStack")]
  public List<string> TechStack { get; set; } = [];

  [JsonProperty("repoUrl")]
  public string RepoUrl { get; set; } = string.Empty;

  [JsonProperty("liveUrl")]
  public string? LiveUrl { get; set; } = string.Empty;

  [JsonProperty("sorter")]
  public int Sorter { get; set; } = 1;

  [JsonIgnore]
  public bool IsDeleted { get; set; } = false;

  [JsonIgnore]
  public DateTime? DeletedAt { get; set; } = null;
}

public class FetchProjectDto
{
  [JsonProperty("id")]
  public string Id { get; set; } = string.Empty;

  [JsonProperty("displayName")]
  public string DisplayName { get; set; } = string.Empty;

  [JsonProperty("shortDescription")]
  public string ShortDescription { get; set; } = string.Empty;

  [JsonProperty("longDescription")]
  public string LongDescription { get; set; } = string.Empty;

  [JsonProperty("techStack")]
  public List<string> TechStack { get; set; } = [];

  [JsonProperty("repoUrl")]
  public string RepoUrl { get; set; } = string.Empty;

  [JsonProperty("liveUrl")]
  public string LiveUrl { get; set; } = string.Empty;
}

public class UpdateProjectDto
{
  [JsonProperty("displayName")]
  public string? DisplayName { get; set; } = null;

  [JsonProperty("shortDescription")]
  public string? ShortDescription { get; set; } = null;

  [JsonProperty("longDescription")]
  public string? LongDescription { get; set; } = null;

  [JsonProperty("techStack")]
  public List<string>? TechStack { get; set; } = null;

  [JsonProperty("repoUrl")]
  public string? RepoUrl { get; set; } = null;

  [JsonProperty("liveUrl")]
  public string? LiveUrl { get; set; } = null;
}