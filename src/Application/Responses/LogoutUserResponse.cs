using System.Net;
using Application.Common;
using Newtonsoft.Json;

namespace Application.Responses;

public class LogoutUserResponse
{
  [JsonProperty("statusCode")]
  public int StatusCode { get; set; } = (int)HttpStatusCode.OK;

  [JsonProperty("responseCode")]
  public string ResponseCode { get; set; } = ResponseCodes.UserLogout;

  [JsonProperty("message")]
  public string Message { get; set; } = "User logged out successfully.";
}