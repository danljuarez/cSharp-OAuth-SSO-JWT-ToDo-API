using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Models.Entities;
using OauthSSOJwtTodoApiBackend.Tests.xUnit.TestUtils;

namespace OauthSSOJwtTodoApiBackend.Tests.xUnit.Services.AuthServiceTests;

public class AuthServiceLogoutAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task LogoutAsync_Should_ReturnSuccess_When_ValidToken()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
        };

        using var context = new TodoDbContext(DbOptions);
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.LogoutAsync(refreshToken.Token);

        // Assert
        Assert.Equal(AuthOperationResult.Success, result);

        var remaining = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken.Token);
        Assert.Null(remaining); // It should be non-existent

        _mockLogger.VerifyLog(LogLevel.Information, "User with refresh token", Times.Once());
    }

    [Fact]
    public async Task LogoutAsync_Should_ReturnLogoutInvalidToken_When_TokenNotFound()
    {
        // Arrange
        var invalidToken = Guid.NewGuid().ToString();
        using var context = new TodoDbContext(DbOptions);
        var service = CreateService(context);

        // Act
        var result = await service.LogoutAsync(invalidToken);

        // Assert
        Assert.Equal(AuthOperationResult.LogoutInvalidToken, result);

        _mockLogger.VerifyLog(LogLevel.Warning, "Logout attempted with invalid refresh token", Times.Once());
    }

    [Fact]
    public async Task LogoutAsync_Should_RemoveExpiredToken_And_ReturnSuccess()
    {
        // Arrange
        var expiredToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Is expired
        };

        using var context = new TodoDbContext(DbOptions);
        await context.RefreshTokens.AddAsync(expiredToken);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.LogoutAsync(expiredToken.Token);

        // Assert
        Assert.Equal(AuthOperationResult.Success, result);
        Assert.Null(await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == expiredToken.Token));

        _mockLogger.VerifyLog(LogLevel.Information, "User with refresh token", Times.Once());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task LogoutAsync_Should_ReturnInvalidToken_When_TokenIsEmptyOrWhitespace(string input)
    {
        // Arrange
        using var context = new TodoDbContext(DbOptions);
        var service = CreateService(context);

        // Act
        var result = await service.LogoutAsync(input);

        // Assert
        Assert.Equal(AuthOperationResult.LogoutInvalidToken, result);

        _mockLogger.VerifyLog(LogLevel.Warning, "Logout attempted with invalid refresh token", Times.Once());
    }
}
