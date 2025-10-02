using Microsoft.Extensions.Logging;
using Moq;
using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Enums;
using OauthSSOJwtTodoApiBackend.Helpers;
using OauthSSOJwtTodoApiBackend.Models.DTOs.Auth;
using OauthSSOJwtTodoApiBackend.Models.Entities;
using OauthSSOJwtTodoApiBackend.Tests.xUnit.TestUtils;
using OauthSSOJwtTodoApiBackend.Tests.xUnit.TestUtils.Providers;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OauthSSOJwtTodoApiBackend.Tests.xUnit.Services.AuthServiceTests;

public class AuthServiceExchangeLinkedInCodeAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task ExchangeLinkedInCodeAsync_Should_ReturnSuccess_When_InDevelopment_And_UserExists()
    {
        // Arrange
        _mockEnv.Setup(e => e.EnvironmentName).Returns("Development");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "devuser",
            Email = "dev@example.com",
            Role = "User",
            HashedPassword = PasswordHasher.HashPassword("devuserpassword")
        };

        using var context = new TodoDbContext(DbOptions);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        _mockJwtHelper.Setup(h => h.GenerateAccessToken(user)).Returns("dev-access-token");

        var service = CreateService(context);

        // Act
        var (data, result) = await service.ExchangeLinkedInCodeAsync("code", "verifier");

        // Assert
        Assert.Equal(AuthOperationResult.Success, result);
        Assert.Equal("dev-access-token", data!.AccessToken);
        _mockLogger.VerifyLog(LogLevel.Information, "Mock LinkedIn login succeeded", Times.Once());
    }

    [Fact]
    public async Task ExchangeLinkedInCodeAsync_Should_ReturnUserNotFound_When_InDevelopment_And_NoUserExists()
    {
        // Arrange
        _mockEnv.Setup(e => e.EnvironmentName).Returns("Development");

        using var context = new TodoDbContext(DbOptions);
        var service = CreateService(context);

        // Act
        var (data, result) = await service.ExchangeLinkedInCodeAsync("code", "verifier");

        // Assert
        Assert.Equal(AuthOperationResult.UserNotFound, result);
        Assert.Null(data);
        _mockLogger.VerifyLog(LogLevel.Error, "Dev LinkedIn login failed", Times.Once());
    }

    [Fact]
    public async Task ExchangeLinkedInCodeAsync_Should_ReturnExchangeFailed_When_TokenRequestFails()
    {
        // Arrange
        _mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

        LinkedInTestHelpers.SetupFakeLinkedInConfig(_mockConfig);

        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.BadRequest));
        HttpClient = new HttpClient(handler);

        using var context = new TodoDbContext(DbOptions);
        var service = CreateService(context);

        // Act
        var (data, result) = await service.ExchangeLinkedInCodeAsync("code", "verifier");

        // Assert
        Assert.Equal(AuthOperationResult.LinkedInExchangeFailed, result);
        Assert.Null(data);
        _mockLogger.VerifyLog(LogLevel.Error, "LinkedIn token exchange failed", Times.Once());
    }

    [Fact]
    public async Task ExchangeLinkedInCodeAsync_Should_ReturnExchangeFailed_When_UserInfoRequestFails()
    {
        // Arrange
        _mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

        LinkedInTestHelpers.SetupFakeLinkedInConfig(_mockConfig);

        var tokenResponse = new LinkedInTokenResponse { AccessToken = "linkedin-token" };
        var tokenJson = JsonSerializer.Serialize(tokenResponse);

        var handler = new FakeHttpMessageHandler(new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(tokenJson, Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.BadRequest)
        });

        HttpClient = new HttpClient(handler);

        using var context = new TodoDbContext(DbOptions);
        var service = CreateService(context);

        // Act
        var (data, result) = await service.ExchangeLinkedInCodeAsync("code", "verifier");

        // Assert
        Assert.Equal(AuthOperationResult.LinkedInExchangeFailed, result);
        Assert.Null(data);
        _mockLogger.VerifyLog(LogLevel.Error, "Failed to retrieve LinkedIn user info", Times.Once());
    }

    [Fact]
    public async Task ExchangeLinkedInCodeAsync_Should_ReturnSuccess_When_UserCreated()
    {
        // Arrange
        _mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

        LinkedInTestHelpers.SetupFakeLinkedInConfig(_mockConfig);

        var tokenResponse = new LinkedInTokenResponse { AccessToken = "linkedin-token" };
        var userResponse = new LinkedInUserInfo
        {
            FirstName = "Test",
            LastName = "User",
            EmailAddress = "testuser@example.com"
        };

        var handler = new FakeHttpMessageHandler(new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userResponse))
            }
        });

        HttpClient = new HttpClient(handler);
        using var context = new TodoDbContext(DbOptions);

        _mockJwtHelper.Setup(h => h.GenerateAccessToken(It.IsAny<User>()))
            .Returns("jwt-token");

        var service = CreateService(context);

        // Act
        var (data, result) = await service.ExchangeLinkedInCodeAsync("code", "verifier");

        // Assert
        Assert.Equal(AuthOperationResult.Success, result);
        Assert.Equal("jwt-token", data!.AccessToken);
        _mockLogger.VerifyLog(LogLevel.Information, "New user created from LinkedIn", Times.Once());
    }

    [Theory]
    [InlineData("ClientId")]
    [InlineData("ClientSecret")]
    [InlineData("RedirectUri")]
    public async Task ExchangeLinkedInCodeAsync_Should_LogError_And_ReturnError_When_RequiredConfigMissing(string missingKey)
    {
        // Arrange
        _mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

        var options = new LinkedInTestConfigOptions
        {
            ClientId = missingKey == "ClientId" ? null : "client-id",
            ClientSecret = missingKey == "ClientSecret" ? null : "client-secret",
            RedirectUri = missingKey == "RedirectUri" ? null : "https://callback"
        };

        LinkedInTestHelpers.SetupFakeLinkedInConfig(_mockConfig, options);

        using var context = new TodoDbContext(DbOptions);
        var service = CreateService(context);

        // Act
        var (data, result) = await service.ExchangeLinkedInCodeAsync("code", "verifier");

        // Assert
        Assert.Equal(AuthOperationResult.Error, result);
        Assert.Null(data);
        _mockLogger.VerifyLog(LogLevel.Error, "LinkedIn exchange failed", Times.Once());
    }

    [Fact]
    public async Task ExchangeLinkedInCodeAsync_Should_ReturnSuccess_When_UserAlreadyExists()
    {
        // Arrange
        _mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

        LinkedInTestHelpers.SetupFakeLinkedInConfig(_mockConfig);

        var tokenResponse = new LinkedInTokenResponse { AccessToken = "linkedin-token" };
        var userResponse = new LinkedInUserInfo
        {
            FirstName = "Existing",
            LastName = "User",
            EmailAddress = "existinguser@example.com"
        };

        var handler = new FakeHttpMessageHandler(new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(userResponse))
            }
        });

        HttpClient = new HttpClient(handler);

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = userResponse.EmailAddress,
            Username = "existinguser",
            Role = "User",
            HashedPassword = PasswordHasher.HashPassword("existinguserpassword")
        };

        using var context = new TodoDbContext(DbOptions);
        await context.Users.AddAsync(existingUser);
        await context.SaveChangesAsync();

        _mockJwtHelper.Setup(h => h.GenerateAccessToken(It.IsAny<User>()))
            .Returns("jwt-token");

        var service = CreateService(context);

        // Act
        var (data, result) = await service.ExchangeLinkedInCodeAsync("code", "verifier");

        // Assert
        Assert.Equal(AuthOperationResult.Success, result);
        Assert.Equal("jwt-token", data!.AccessToken);

        _mockLogger.VerifyLog(LogLevel.Information, "LinkedIn login for existing user", Times.Once());
    }
}
