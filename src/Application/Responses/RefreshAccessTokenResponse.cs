using System.Net;
using Application.Common;
using Newtonsoft.Json;

namespace Application.Responses;

public class RefreshAccessTokenResponse(string newAccessToken)
{
  [JsonProperty("statusCode")]
  public int StatusCode { get; set; } = (int)HttpStatusCode.OK;

  [JsonProperty("responseCode")]
  public string ResponseCode { get; set; } = ResponseCodes.TokenRefresh;

  [JsonProperty("message")]
  public string Message { get; set; } = "Access token renewed successfully.";

  [JsonProperty("newAccessToken")]
  public string NewAccessToken { get; set; } = newAccessToken;
}