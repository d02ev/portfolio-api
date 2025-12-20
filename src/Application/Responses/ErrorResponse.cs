using System.Net;
using System.Text.Json.Serialization;
using Application.Common;
using Newtonsoft.Json;

namespace Application.Responses;

public class ErrorResponse
{
  [JsonProperty("statusCode")]
  public int StatusCode { get; set; } = (int)HttpStatusCode.InternalServerError;

  [JsonProperty("responseCode")]
  public string ResponseCode { get; set; } = ResponseCodes.InternalServerError;

  [JsonProperty("resourceName")]
  public string ResourceName { get; set; } = ResourceNames.App;

  [JsonProperty("message")]
  public string Message { get; set; } = "Something went wrong. Please try again later.";
}