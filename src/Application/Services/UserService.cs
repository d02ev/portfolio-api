using System.Security.Claims;
using Application.Common;
using Application.Dto;
using Application.Helpers;
using Application.Repositories;
using Application.Responses;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.Services;

public class UserService(IUserRepository userRepository, IAuthHelper authHelper, ILogger<UserService> logger) : IUserService
{
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IAuthHelper _authHelper = authHelper;
  private readonly ILogger<UserService> _logger = logger;

  public async Task<LoginUserResponse> LoginUser(UserDto userDto)
  {
    _logger.LogInformation("Started {Operation}.", nameof(LoginUser));
    try
    {
      var username = userDto.Username;
      var password = userDto.Password;

      var userExists = await _userRepository.FetchByUsernameAsync(username);
      if (userExists is null)
      {
        _logger.LogWarning("User not found during login.");
        throw new NotFoundException(ResourceNames.User, username);
      }

      var passwordHash = userExists.PasswordHash;
      if (!_authHelper.ComparePassword(password, passwordHash))
      {
        _logger.LogWarning("Invalid credentials during login.");
        throw new BadRequestException(ResourceNames.User, "Invalid credentials.");
      }

      var accessTokenClaims = new List<Claim>
      {
        new(ClaimTypes.NameIdentifier, userExists.Id),
      };
      var refreshTokenClaims = new List<Claim>
      {
        new(ClaimTypes.NameIdentifier, userExists.Id),
      };
      var accessToken = _authHelper.GenerateAccessToken(accessTokenClaims.AsEnumerable());
      var refreshToken = _authHelper.GenerateRefreshToken(refreshTokenClaims.AsEnumerable());
      var serializedChanges = JsonConvert.SerializeObject(new { RefreshToken = refreshToken });

      await _userRepository.UpdateAsync(userExists.Id, serializedChanges);

      _logger.LogInformation("Completed {Operation} for UserId={UserId}.", nameof(LoginUser), userExists.Id);
      return new LoginUserResponse(accessToken, refreshToken);
    }
    catch (BadRequestException)
    {
      throw;
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}.", nameof(LoginUser));
      throw;
    }
  }

  public async Task<RefreshAccessTokenResponse> RefreshAccessToken(string refreshToken)
  {
    _logger.LogInformation("Started {Operation}.", nameof(RefreshAccessToken));
    try
    {
      var decodedToken = _authHelper.DecodeToken(refreshToken, "refresh");
      if (decodedToken is null)
      {
        _logger.LogWarning("Refresh token decode failed.");
        throw new BadRequestException(ResourceNames.User, "Cannot refresh the access token, refresh token is invalid or has expired. Please login again.");
      }

      var userId = decodedToken.FindFirst(ClaimTypes.NameIdentifier)!.Value;
      var user = await _userRepository.FetchByIdAsync(userId);
      if (user is null)
      {
        _logger.LogWarning("User not found during token refresh.");
        throw new NotFoundException(ResourceNames.User, userId);
      }

      var accessTokenClaims = new List<Claim>
      {
        new(ClaimTypes.NameIdentifier, userId),
      };
      var accessToken = _authHelper.GenerateAccessToken(accessTokenClaims.AsEnumerable());

      _logger.LogInformation("Completed {Operation} for UserId={UserId}.", nameof(RefreshAccessToken), userId);
      return new RefreshAccessTokenResponse(accessToken);
    }
    catch (BadRequestException)
    {
      throw;
    }
    catch (NotFoundException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}.", nameof(RefreshAccessToken));
      throw;
    }
  }

  public async Task<CreateResourceResponse<IDictionary<string, string>>> RegisterUser(UserDto userDto)
  {
    _logger.LogInformation("Started {Operation}.", nameof(RegisterUser));
    try
    {
      var userExists = await _userRepository.FetchByUsernameAsync(userDto.Username);
      if (userExists is not null)
      {
        _logger.LogWarning("Duplicate user detected during register.");
        throw new BadRequestException(ResourceNames.User, $"User with username '{userDto.Username}' already exists.");
      }

      var passwordHash = _authHelper.GeneratePasswordHash(userDto.Password);
      var user = new User
      {
        Username = userDto.Username,
        PasswordHash = passwordHash,
      };

      await _userRepository.CreateAsync(user);
      var newUser = await _userRepository.FetchByUsernameAsync(userDto.Username);
      if (newUser is null)
      {
        throw new InternalServerException(ResourceNames.User, "An error occurred while creating the user.");
      }
      var data = new Dictionary<string, string>
      {
        { "id", newUser.Id }
      };

      _logger.LogInformation("Completed {Operation} for UserId={UserId}.", nameof(RegisterUser), newUser.Id);
      return new CreateResourceResponse<IDictionary<string, string>>(ResourceNames.User, data);
    }
    catch (BadRequestException)
    {
      throw;
    }
    catch (InternalServerException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error in {Operation}.", nameof(RegisterUser));
      throw;
    }
  }
}
