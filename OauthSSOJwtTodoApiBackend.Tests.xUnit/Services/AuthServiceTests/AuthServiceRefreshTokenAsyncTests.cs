using Microsoft.Extensions.Logging;
using Moq;
using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Helpers;
using OauthSSOJwtTodoApiBackend.Models.Entities;
using OauthSSOJwtTodoApiBackend.Tests.xUnit.TestUtils;

namespace OauthSSOJwtTodoApiBackend.Tests.xUnit.Services.AuthServiceTests;

public class AuthServiceRefreshTokenAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task RefreshTokenAsync_Should_ReturnSuccess_When_TokenIsValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "test",
            Email = "test@example.com",
            Role = "User",
            HashedPassword = PasswordHasher.HashPassword("testpassword")
        };

        var oldToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        using var context = new TodoDbContext(DbOptions);
        await context.Users.AddAsync(user);
        await context.RefreshTokens.AddAsync(oldToken);
        await context.SaveChangesAsync();

        _mockJwtHelper.Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
                      .Returns("new-mock-access-token");

        var service = CreateService(context);

        // Act
        var (data, result) = await service.RefreshTokenAsync(oldToken.Token);

        // Assert
        Assert.Equal(AuthOperationResult.Success, result);
        Assert.NotNull(data);
        Assert.Equal("new-mock-access-token", data!.AccessToken);
        Assert.NotEqual(oldToken.Token, data.RefreshToken);
        Assert.Equal(_tokenSettings.AccessTokenExpirationMinutes * 60, data.ExpiresIn);

        _mockLogger.VerifyLog(LogLevel.Warning, "Refresh token failed: invalid or expired token", Times.Never());
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_ReturnTokenInvalid_When_TokenIsExpired()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "test",
            Email = "test@example.com",
            Role = "User",
            HashedPassword = PasswordHasher.HashPassword("testpassword")
        };

        var expiredToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        using var context = new TodoDbContext(DbOptions);
        await context.Users.AddAsync(user);
        await context.RefreshTokens.AddAsync(expiredToken);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var (data, result) = await service.RefreshTokenAsync(expiredToken.Token);

        // Assert
        Assert.Equal(AuthOperationResult.TokenInvalid, result);
        Assert.Null(data);

        _mockLogger.VerifyLog(LogLevel.Warning, "Refresh token failed: invalid or expired token", Times.Once());
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_ReturnTokenInvalid_When_UserIsNull()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(), // It does not exist in DB
            User = null!, // User is missing
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        using var context = new TodoDbContext(DbOptions);
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var (data, result) = await service.RefreshTokenAsync(refreshToken.Token);

        // Assert
        Assert.Equal(AuthOperationResult.TokenInvalid, result);
        Assert.Null(data);

        _mockLogger.VerifyLog(LogLevel.Warning, "Refresh token failed: invalid or expired token", Times.Once());
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_ReturnTokenInvalid_When_TokenNotFound()
    {
        // Arrange
        using var context = new TodoDbContext(DbOptions);
        var service = CreateService(context);

        var nonExistentToken = Guid.NewGuid().ToString();

        // Act
        var (data, result) = await service.RefreshTokenAsync(nonExistentToken);

        // Assert
        Assert.Equal(AuthOperationResult.TokenInvalid, result);
        Assert.Null(data);

        _mockLogger.VerifyLog(LogLevel.Warning, "Refresh token failed: invalid or expired token", Times.Once());
    }
}
