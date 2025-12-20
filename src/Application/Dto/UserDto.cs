using Newtonsoft.Json;

namespace Application.Dto;

public class UserDto
{
  [JsonProperty("username")]
  public string Username { get; set; } = string.Empty;

  [JsonProperty("password")]
  public string Password { get; set; } = string.Empty;
}