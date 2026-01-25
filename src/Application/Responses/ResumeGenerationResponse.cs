using Newtonsoft.Json;

namespace Application.Responses;

public class ResumeGenerationResponse
{
  [JsonProperty("jobId")]
  public long JobId { get; set; }

  [JsonProperty("resumeName")]
  public string ResumeName { get; set; } = string.Empty;

  [JsonProperty("latexFileName")]
  public string LatexFileName { get; set; } = string.Empty;
}