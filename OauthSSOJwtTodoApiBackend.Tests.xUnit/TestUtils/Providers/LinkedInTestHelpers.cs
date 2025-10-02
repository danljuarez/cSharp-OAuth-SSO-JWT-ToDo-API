using Microsoft.Extensions.Configuration;
using Moq;

namespace OauthSSOJwtTodoApiBackend.Tests.xUnit.TestUtils.Providers;

public class LinkedInTestConfigOptions
{
    public string? ClientId { get; init; } = "test-client-id";
    public string? ClientSecret { get; init; } = "test-client-secret";
    public string? RedirectUri { get; init; } = "https://localhost/callback";
    public string? TokenEndpoint { get; init; } = "https://linkedin.com/token";
    public string? UserInfoEndpoint { get; init; } = "https://linkedin.com/user";
}

public static class LinkedInTestHelpers
{
    public static void SetupFakeLinkedInConfig(
        Mock<IConfiguration> mockConfig,
        LinkedInTestConfigOptions? options = null)
    {
        var config = options ?? new LinkedInTestConfigOptions();

        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s["ClientId"]).Returns(config.ClientId);
        mockSection.Setup(s => s["ClientSecret"]).Returns(config.ClientSecret);
        mockSection.Setup(s => s["RedirectUri"]).Returns(config.RedirectUri);
        mockSection.Setup(s => s["TokenEndpoint"]).Returns(config.TokenEndpoint);
        mockSection.Setup(s => s["UserInfoEndpoint"]).Returns(config.UserInfoEndpoint);

        mockConfig.Setup(c => c.GetSection("LinkedIn")).Returns(mockSection.Object);
    }
}
