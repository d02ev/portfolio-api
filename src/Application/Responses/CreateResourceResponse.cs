using System.Net;
using Application.Common;
using Newtonsoft.Json;

namespace Application.Responses;

public class CreateResourceResponse<T>(string resourceName, T data) where T : class
{
  [JsonProperty("statusCode")]
  public int StatusCode { get; set; } = (int)HttpStatusCode.Created;

  [JsonProperty("responseCode")]
  public string ResponseCode { get; set; } = ResponseCodes.ResourceCreated;

  [JsonProperty("resourceName")]
  public string ResourceName { get; set; } = resourceName;

  [JsonProperty("message")]
  public string Message { get; set; } = string.Empty;

  [JsonProperty("data")]
  public T Data { get; set; } = data;
}

public class CreateResourceResponse(string resourceName)
{
  [JsonProperty("statusCode")]
  public int StatusCode { get; set; } = (int)HttpStatusCode.Created;

  [JsonProperty("responseCode")]
  public string ResponseCode { get; set; } = ResponseCodes.ResourceCreated;

  [JsonProperty("resourceName")]
  public string ResourceName { get; set; } = resourceName;
}