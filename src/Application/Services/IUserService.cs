using Application.Dto;
using Application.Responses;

namespace Application.Services;

public interface IUserService
{
  Task<CreateResourceResponse<IDictionary<string, string>>> RegisterUser(UserDto userDto);

  Task<LoginUserResponse> LoginUser(UserDto userDto);

  Task<ShortLivedTokenResponse> GenerateShortLivedToken(string username);

  Task<RefreshAccessTokenResponse> RefreshAccessToken(string refreshToken);
}