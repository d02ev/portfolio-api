using System.Net;
using Application.Common;
using Newtonsoft.Json;

namespace Application.Responses;

public class DeleteResourceResponse(string resourceName, string resourceId)
{
  [JsonProperty("statusCode")]
  public int StatusCode { get; set; } = (int)HttpStatusCode.OK;

  [JsonProperty("responseCode")]
  public string ResponseCode { get; set; } = ResponseCodes.ResourceDeleted;

  [JsonProperty("resourceName")]
  public string ResourceName { get; set; } = resourceName;

  [JsonProperty("message")]
  public string Message { get; set; } = $"{resourceName} deleted successfully.";

  [JsonProperty("data")]
  public IDictionary<string, string> Data { get; set; } = new Dictionary<string, string> { { "resourceId", resourceId } };
}