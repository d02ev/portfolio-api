using System.Net;
using System.Text.Json;
using Application.Common;
using Newtonsoft.Json;

namespace Application.Responses;

public class LoginUserResponse(string accessToken, string refreshToken)
{
  [JsonProperty("statusCode")]
  public int StatusCode { get; set; } = (int)HttpStatusCode.OK;

  [JsonProperty("responseCode")]
  public string ResponseCode { get; set; } = ResponseCodes.UserLogin;

  [JsonProperty("message")]
  public string Message { get; set; } = "User logged in successfully.";

  [JsonProperty("token")]
  public TokenResponse Token { get; set; } = new TokenResponse(accessToken, refreshToken);
}

public class TokenResponse(string accessToken, string refreshToken) {
  [JsonProperty("accessToken")]
  public string AccessToken { get; set; } = accessToken;

  [JsonProperty("refreshToken")]
  public string RefreshToken { get; set; } = refreshToken;
}