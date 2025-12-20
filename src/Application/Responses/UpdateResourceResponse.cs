using System.Net;
using Application.Common;
using Newtonsoft.Json;

namespace Application.Responses;

public class UpdateResourceResponse<T>(string resourceName, T data) where T : class
{
  [JsonProperty("statusCode")]
  public int StatusCode { get; set; } = (int)HttpStatusCode.OK;

  [JsonProperty("responseCode")]
  public string ResponseCode { get; set; } = ResponseCodes.ResourceUpdated;

  [JsonProperty("resourceName")]
  public string ResourceName { get; set; } = resourceName;

  [JsonProperty("data")]
  public T Data { get; set; } = data;
}