using System.Net;
using Newtonsoft.Json;

namespace Application.Responses;

public class SendMessageResponse
{
  [JsonProperty("statusCode")]
  public int StatusCode { get; set; } = (int)HttpStatusCode.OK;

  [JsonProperty("message")]
  public string Message { get; set; } = "Message sent successfully.";
}