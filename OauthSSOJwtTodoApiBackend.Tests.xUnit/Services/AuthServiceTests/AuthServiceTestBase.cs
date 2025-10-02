using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OauthSSOJwtTodoApiBackend.Configuration;
using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Helpers;
using OauthSSOJwtTodoApiBackend.Services;

namespace OauthSSOJwtTodoApiBackend.Tests.xUnit.Services.AuthServiceTests;

public abstract class AuthServiceTestBase
{
    protected readonly Mock<IJwtHelper> _mockJwtHelper = new();
    protected readonly Mock<ILogger<AuthService>> _mockLogger = new();
    protected readonly Mock<IWebHostEnvironment> _mockEnv = new();
    protected readonly Mock<IConfiguration> _mockConfig = new();
    protected readonly Mock<IOptions<TokenSettings>> _mockTokenOptions = new();
    protected readonly TokenSettings _tokenSettings;
    protected HttpClient HttpClient { get; set; } = new HttpClient();

    protected readonly DbContextOptions<TodoDbContext> DbOptions;

    protected AuthServiceTestBase()
    {
        _tokenSettings = new TokenSettings
        {
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        _mockTokenOptions.Setup(o => o.Value).Returns(_tokenSettings);

        DbOptions = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Guid.NewGuid() ensures isolation per test
            .Options;
    }

    protected AuthService CreateService(TodoDbContext context)
    {
        return new AuthService(
            context,
            _mockJwtHelper.Object,
            _mockLogger.Object,
            _mockEnv.Object,
            _mockConfig.Object,
            _mockTokenOptions.Object,
            HttpClient
        );
    }
}