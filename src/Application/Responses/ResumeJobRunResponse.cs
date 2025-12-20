using System.Net;
using Newtonsoft.Json;

namespace Application.Responses;

public class ResumeJobRunResponse
{
  [JsonProperty("statusCode")]
  public int StatusCode { get; set; } = (int)HttpStatusCode.OK;

  [JsonProperty("responseCode")]
  public string ResponseCode { get; set; } = "JOB_STATUS_FETCHED";

  [JsonProperty("resourceName")]
  public string ResourceName { get; set; } = "ResumeJobRun";

  [JsonProperty("status")]
  public string Status { get; set; } = "pending";

  [JsonProperty("pdfUrl")]
  public string? PdfUrl { get; set; } = null;

  [JsonProperty("error")]
  public string? Error { get; set; } = null;
}