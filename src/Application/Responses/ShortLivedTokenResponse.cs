using System.Net;
using Newtonsoft.Json;

namespace Application.Responses;

public class ShortLivedTokenResponse(string token)
{
  [JsonProperty("statusCode")]
  public int StatusCode { get; set; } = (int)HttpStatusCode.OK;

  [JsonProperty("responseCode")]
  public string ResponseCode { get; set; } = "SHORT_LIVED_TOKEN_GENERATED";

  [JsonProperty("resourceName")]
  public string ResourceName { get; set; } = "TOKEN";

  [JsonProperty("token")]
  public string Token { get; set; } = token;
}