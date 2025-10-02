using Microsoft.Extensions.Logging;
using Moq;
using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Helpers;
using OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;
using OauthSSOJwtTodoApiBackend.Models.Entities;
using OauthSSOJwtTodoApiBackend.Tests.xUnit.TestUtils;

namespace OauthSSOJwtTodoApiBackend.Tests.xUnit.Services.AuthServiceTests;

public class AuthServiceLoginAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task LoginAsync_Should_ReturnSuccess_When_CredentialsAreValid()
    {
        // Arrange
        var password = "password123";
        var hashedPassword = PasswordHasher.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "test",
            Email = "test@example.com",
            HashedPassword = hashedPassword,
            Role = "User"
        };

        using var context = new TodoDbContext(DbOptions);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        _mockJwtHelper.Setup(h => h.GenerateAccessToken(user)).Returns("access-token");

        var request = new LoginRequestDto
        {
            UsernameOrEmail = "test",
            Password = password
        };

        // Act
        var (result, status) = await service.LoginAsync(request);

        // Assert
        Assert.Equal(AuthOperationResult.Success, status);
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.Equal(_tokenSettings.AccessTokenExpirationMinutes * 60, result.ExpiresIn);

        _mockLogger.VerifyLog(LogLevel.Information, "logged in with AccessToken", Times.Once());
    }

    [Fact]
    public async Task LoginAsync_Should_ReturnInvalidCredentials_When_UserNotFound()
    {
        using var context = new TodoDbContext(DbOptions);
        var service = CreateService(context);

        var request = new LoginRequestDto
        {
            UsernameOrEmail = "unknownuser",
            Password = "unknownuserpassword"
        };

        var (result, status) = await service.LoginAsync(request);

        Assert.Equal(AuthOperationResult.InvalidCredentials, status);
        Assert.Null(result);

        _mockLogger.VerifyLog(LogLevel.Warning, "invalid credentials", Times.Once());
    }

    [Fact]
    public async Task LoginAsync_Should_ReturnInvalidCredentials_When_PasswordIncorrect()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "test",
            Email = "test@example.com",
            HashedPassword = PasswordHasher.HashPassword("test-password"),
            Role = "User"
        };

        using var context = new TodoDbContext(DbOptions);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var request = new LoginRequestDto
        {
            UsernameOrEmail = "test",
            Password = "other-password"
        };

        var (result, status) = await service.LoginAsync(request);

        Assert.Equal(AuthOperationResult.InvalidCredentials, status);
        Assert.Null(result);

        _mockLogger.VerifyLog(LogLevel.Warning, "invalid credentials", Times.Once());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("InvalidRole")]
    public async Task LoginAsync_Should_ReturnInvalidCredentials_When_RoleInvalid(string? role)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "test",
            Email = "test@example.com",
            HashedPassword = PasswordHasher.HashPassword("testpassword"),
            Role = role!
        };

        using var context = new TodoDbContext(DbOptions);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var request = new LoginRequestDto
        {
            UsernameOrEmail = "test",
            Password = "testpassword"
        };

        var (result, status) = await service.LoginAsync(request);

        Assert.Equal(AuthOperationResult.InvalidCredentials, status);
        Assert.Null(result);

        _mockLogger.VerifyLog(LogLevel.Warning, "missing or invalid role", Times.Once());
    }
}
