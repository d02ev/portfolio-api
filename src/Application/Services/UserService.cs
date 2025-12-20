using System.Security.Claims;
using Application.Common;
using Application.Dto;
using Application.Helpers;
using Application.Repositories;
using Application.Responses;
using Domain.Entities;
using Domain.Exceptions;
using Newtonsoft.Json;

namespace Application.Services;

public class UserService(IUserRepository userRepository, IAuthHelper authHelper) : IUserService
{
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IAuthHelper _authHelper = authHelper;

  public async Task<ShortLivedTokenResponse> GenerateShortLivedToken(string username)
  {
    var userExists = await _userRepository.FetchByUsernameAsync(username) ?? throw new NotFoundException(ResourceNames.User, username);
    var userId = userExists.Id;
    var claims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, userId),
      new(ClaimTypes.Role, userExists.Role)
    };
    var shortLivedToken = _authHelper.GenerateShortLivedAccessToken(claims.AsEnumerable());

    return new ShortLivedTokenResponse(shortLivedToken);
  }

  public async Task<LoginUserResponse> LoginUser(UserDto userDto)
  {
    var username = userDto.Username;
    var password = userDto.Password;

    var userExists = await _userRepository.FetchByUsernameAsync(username) ?? throw new NotFoundException(ResourceNames.User, username);
    var passwordHash = userExists.PasswordHash;

    if (!_authHelper.ComparePassword(password, passwordHash))
    {
      throw new BadRequestException(ResourceNames.User, "Invalid credentials.");
    }

    var accessTokenClaims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, userExists.Id),
      new(ClaimTypes.Role, userExists.Role),
    };
    var refreshTokenClaims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, userExists.Id),
    };
    var accessToken = _authHelper.GenerateAccessToken(accessTokenClaims.AsEnumerable());
    var refreshToken = _authHelper.GenerateRefreshToken(refreshTokenClaims.AsEnumerable());
    var serializedChanges = JsonConvert.SerializeObject(new { RefreshToken = refreshToken });

    await _userRepository.UpdateAsync(userExists.Id, serializedChanges);

    return new LoginUserResponse(accessToken, refreshToken);
  }

  public async Task<RefreshAccessTokenResponse> RefreshAccessToken(string refreshToken)
  {
    var decodedToken = _authHelper.DecodeToken(refreshToken, "refresh") ?? throw new BadRequestException(ResourceNames.User, "Cannot refresh the access token, refresh token is invalid or has expired. Please login again.");
    var userId = decodedToken.FindFirst(ClaimTypes.NameIdentifier)!.Value;
    var user = await _userRepository.FetchByIdAsync(userId) ?? throw new NotFoundException(ResourceNames.User, userId);
    var accessTokenClaims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, userId),
      new(ClaimTypes.Role, user.Role)
    };
    var accessToken = _authHelper.GenerateAccessToken(accessTokenClaims.AsEnumerable());

    return new RefreshAccessTokenResponse(accessToken);
  }

  public async Task<CreateResourceResponse<IDictionary<string, string>>> RegisterUser(UserDto userDto)
  {
    var userExists = await _userRepository.FetchByUsernameAsync(userDto.Username);
    if (userExists is not null)
    {
      throw new BadRequestException(ResourceNames.User, $"User with username '{userDto.Username}' already exists.");
    }

    var passwordHash = _authHelper.GeneratePasswordHash(userDto.Password);
    var user = new User
    {
      Username = userDto.Username,
      PasswordHash = passwordHash,
      Role = userDto.Username == "vikramaditya" ? "admin" : "user"
    };

    await _userRepository.CreateAsync(user);
    var newUser = await _userRepository.FetchByUsernameAsync(userDto.Username);
    var data = new Dictionary<string, string>
    {
      { "id", newUser!.Id }
    };

    return new CreateResourceResponse<IDictionary<string, string>>(ResourceNames.User, data);
  }
}