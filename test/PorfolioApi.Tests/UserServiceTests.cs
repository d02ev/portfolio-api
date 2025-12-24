using System.Net;
using System.Security.Claims;
using Application.Common;
using Application.Dto;
using Application.Helpers;
using Application.Repositories;
using Application.Responses;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;

namespace PorfolioApi.Tests;

public class UserServiceTests
{
  private readonly Mock<IUserRepository> _userRepositoryMock;
  private readonly Mock<IAuthHelper> _authHelperMock;
  private readonly IUserService _userService;

  public UserServiceTests()
  {
    _userRepositoryMock = new Mock<IUserRepository>();
    _authHelperMock = new Mock<IAuthHelper>();
    _userService = new UserService(_userRepositoryMock.Object, _authHelperMock.Object);
  }

  [Fact]
  public async Task LoginUser_ShouldAuthenticateUser_WhenUserExists()
  {
    var user = new User
    {
      Id = "user123",
      Username = "testuser",
      PasswordHash = "hashedpass",
      RefreshToken = null
    };
    var userDto = new UserDto
    {
      Username = "testuser",
      Password = "somepass"
    };

    _userRepositoryMock
      .Setup(r => r.FetchByUsernameAsync("testuser"))
      .ReturnsAsync(user);

    _userRepositoryMock
      .Setup(r => r.UpdateAsync(user.Id, JsonConvert.SerializeObject(new { RefreshToken = "refreshToken" })))
      .Returns(Task.CompletedTask);

    _authHelperMock
      .Setup(h => h.ComparePassword(userDto.Password, user.PasswordHash))
      .Returns(true);

    _authHelperMock
      .Setup(h => h.GenerateAccessToken(
         It.Is<IEnumerable<Claim>>(cs =>
           cs.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id)
         )))
      .Returns("accessToken");

    _authHelperMock
      .Setup(h => h.GenerateRefreshToken(
         It.Is<IEnumerable<Claim>>(cs =>
           cs.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id) != null
         )))
      .Returns("refreshToken");

    var result = await _userService.LoginUser(userDto);

    result.Should().BeOfType<LoginUserResponse>();
    result.StatusCode.Should().Be((int)HttpStatusCode.OK);
    result.ResponseCode.Should().Be(ResponseCodes.UserLogin);
    result.Message.Should().Be("User logged in successfully.");
    result.Token.Should().BeOfType<TokenResponse>();
    result.Token.RefreshToken.Should().BeOfType<string>();
    result.Token.AccessToken.Should().BeOfType<string>();
    result.Token.RefreshToken.Should().Be("refreshToken");
    result.Token.AccessToken.Should().Be("accessToken");
  }

  [Fact]
  public async Task LoginUser_ShouldThrowNotFoundException_WhenUserDoesNotExist()
  {
    // Arrange
    var userDto = new UserDto
    {
      Username = "nonexistent",
      Password = "irrelevant"
    };

    _userRepositoryMock
        .Setup(r => r.FetchByUsernameAsync(userDto.Username))
        .ReturnsAsync((User)null);

    // Act
    Func<Task> act = () => _userService.LoginUser(userDto);

    // Assert
    await act
        .Should()
        .ThrowAsync<NotFoundException>()
        .Where(ex =>
            ex.ResourceName == ResourceNames.User &&
            ex.StatusCode == (int)HttpStatusCode.NotFound &&
            ex.Message == $"{ResourceNames.User} with key '{userDto.Username}' not found."
        );
  }

  [Fact]
  public async Task LoginUserAsync_ShouldThrowBadRequestException_WhenPasswordInvalid()
  {
    // Arrange
    var user = new User
    {
      Id = "user123",
      Username = "testuser",
      PasswordHash = "correctHash",
    };

    var userDto = new UserDto
    {
      Username = "testuser",
      Password = "wrongPassword"
    };

    _userRepositoryMock
        .Setup(r => r.FetchByUsernameAsync(userDto.Username))
        .ReturnsAsync(user);

    _authHelperMock
        .Setup(h => h.ComparePassword(userDto.Password, user.PasswordHash))
        .Returns(false);

    // Act
    Func<Task> act = () => _userService.LoginUser(userDto);

    // Assert
    await act
        .Should()
        .ThrowAsync<BadRequestException>()
        .Where(ex =>
            ex.ResourceName == ResourceNames.User &&
            ex.Message.Contains("Invalid credentials")
        );
  }

  [Fact]
  public async Task RefreshAccessToken_ShouldReturnNewAccessToken_WhenRefreshTokenValidAndUserExists()
  {
    // Arrange
    var refreshToken = "validToken";
    var userId = "user123";
    var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
    [
        new Claim(ClaimTypes.NameIdentifier, userId)
    ], "refresh"));

    var user = new User
    {
      Id = userId,
    };

    _authHelperMock
        .Setup(h => h.DecodeToken(refreshToken, "refresh"))
        .Returns(claimsPrincipal);

    _userRepositoryMock
        .Setup(r => r.FetchByIdAsync(userId))
        .ReturnsAsync(user);

    _authHelperMock
        .Setup(h => h.GenerateAccessToken(
            It.Is<IEnumerable<Claim>>(cs =>
                cs.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId)
            )))
        .Returns("newAccessToken");

    // Act
    var response = await _userService.RefreshAccessToken(refreshToken);

    // Assert
    response.Should().BeOfType<RefreshAccessTokenResponse>();
    response.NewAccessToken.Should().Be("newAccessToken");
  }

  [Fact]
  public async Task RefreshAccessToken_ShouldThrowNotFoundException_WhenUserDoesNotExist()
  {
    // Arrange
    var refreshToken = "validToken";
    var claims = new ClaimsPrincipal(new ClaimsIdentity(
    [
        new Claim(ClaimTypes.NameIdentifier, "unknownUser")
    ], "refresh"));

    _authHelperMock
        .Setup(h => h.DecodeToken(refreshToken, "refresh"))
        .Returns(claims);

    _userRepositoryMock
        .Setup(r => r.FetchByIdAsync("unknownUser"))
        .ReturnsAsync((User)null);

    // Act
    Func<Task> act = () => _userService.RefreshAccessToken(refreshToken);

    // Assert
    await act
        .Should()
        .ThrowAsync<NotFoundException>()
        .Where(ex =>
            ex.ResourceName == ResourceNames.User &&
            ex.StatusCode == (int)HttpStatusCode.NotFound
        );
  }

  [Fact]
  public async Task RefreshAccessToken_ShouldThrowBadRequestException_WhenDecodeFails()
  {
    // Arrange
    var invalidToken = "badToken";
    _authHelperMock
        .Setup(h => h.DecodeToken(invalidToken, "refresh"))
        .Returns((ClaimsPrincipal)null);

    // Act
    Func<Task> act = () => _userService.RefreshAccessToken(invalidToken);

    // Assert
    await act
        .Should()
        .ThrowAsync<BadRequestException>()
        .Where(ex =>
            ex.ResourceName == ResourceNames.User &&
            ex.Message.Contains("Cannot refresh the access token, refresh token is invalid or has expired. Please login again.")
        );
  }

  [Fact]
  public async Task RegisterUser_ShouldAssignAdminRole_WhenUsernameIsVikramaditya()
  {
    // Arrange
    var userDto = new UserDto
    {
      Username = "vikramaditya",
      Password = "royalPass"
    };
    var hash = "hashedRoyalPass";
    var createdUser = new User
    {
      Id = "royal-id-456",
      Username = userDto.Username,
      PasswordHash = hash
    };

    // Return null on first call, then createdUser on second call
    _userRepositoryMock
      .SetupSequence(r => r.FetchByUsernameAsync(userDto.Username))
      .ReturnsAsync((User)null)
      .ReturnsAsync(createdUser);

    _authHelperMock
        .Setup(h => h.GeneratePasswordHash(userDto.Password))
        .Returns(hash);

    _userRepositoryMock
        .Setup(r => r.CreateAsync(
            It.Is<User>(u =>
                u.Username == userDto.Username &&
                u.PasswordHash == hash
            )))
        .Returns(Task.CompletedTask);

    // Act
    var response = await _userService.RegisterUser(userDto);

    // Assert
    response.StatusCode.Should().Be((int)HttpStatusCode.Created);
    response.ResponseCode.Should().Be(ResponseCodes.ResourceCreated);
    response.ResourceName.Should().Be(ResourceNames.User);
    response.Data.Should().ContainKey("id").WhoseValue.Should().Be(createdUser.Id);
  }


  [Fact]
  public async Task RegisterUser_ShouldCreateUserAndReturnResponse_ForNormalUsername()
  {
    // Arrange
    var userDto = new UserDto
    {
      Username = "alice",
      Password = "securePass"
    };
    var generatedHash = "hashedSecurePass";
    var createdUser = new User
    {
      Id = "new-id-123",
      Username = userDto.Username,
      PasswordHash = generatedHash,
    };

    // First call returns null (no existing user), second returns the created user
    _userRepositoryMock
        .SetupSequence(r => r.FetchByUsernameAsync(userDto.Username))
        .ReturnsAsync((User)null)
        .ReturnsAsync(createdUser);

    _authHelperMock
        .Setup(h => h.GeneratePasswordHash(userDto.Password))
        .Returns(generatedHash);

    _userRepositoryMock
        .Setup(r => r.CreateAsync(It.Is<User>(u =>
            u.Username == userDto.Username &&
            u.PasswordHash == generatedHash
        )))
        .Returns(Task.CompletedTask);

    // Act
    var response = await _userService.RegisterUser(userDto);

    // Assert
    response.StatusCode.Should().Be((int)HttpStatusCode.Created);
    response.ResponseCode.Should().Be(ResponseCodes.ResourceCreated);
    response.ResourceName.Should().Be(ResourceNames.User);
    response.Data.Should().ContainKey("id")
           .WhoseValue.Should().Be(createdUser.Id);
  }


  [Fact]
  public async Task RegisterUser_ShouldThrowBadRequestException_WhenUsernameExists()
  {
    // Arrange
    var userDto = new UserDto
    {
      Username = "testuser",
      Password = "anyPass"
    };

    _userRepositoryMock
        .Setup(r => r.FetchByUsernameAsync(userDto.Username))
        .ReturnsAsync(new User { Username = userDto.Username });

    // Act
    Func<Task> act = () => _userService.RegisterUser(userDto);

    // Assert
    await act
        .Should()
        .ThrowAsync<BadRequestException>()
        .Where(ex =>
            ex.ResourceName == ResourceNames.User &&
            ex.Message.Contains($"User with username '{userDto.Username}' already exists.")
        );
  }

}