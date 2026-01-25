using Application.Common;
using Newtonsoft.Json;

namespace Application.Requests;

public class SendMessageRequest
{
  [JsonProperty("pdfUrl")]
  public string? PdfUrl { get; set; } = null;

  [JsonProperty("companyName")]
  public string? CompanyName { get; set; } = null;

  [JsonProperty("mode")]
  public string Mode { get; set; } = ResumeModes.Generic;

  [JsonProperty("errorMessage")]
  public string? ErrorMessage { get; set; } = null;
}
