using OauthSSOJwtTodoApiBackend.Configuration;

namespace OauthSSOJwtTodoApiBackend.Helpers;

public static class LinkedInConfigValidator
{
    public static void ValidateLinkedInConfig(WebApplication app)
    {
        if (!app.Environment.IsProduction())
            return;

        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var config = app.Configuration.GetSection("LinkedIn").Get<LinkedInOAuthSettings>();

        if (config == null)
        {
            logger.LogError("LinkedIn configuration is missing.");
            throw new InvalidOperationException("LinkedIn configuration is missing.");
        }

        var missingKeys = new List<string>();

        if (string.IsNullOrWhiteSpace(config.ClientId)) missingKeys.Add(nameof(config.ClientId));
        if (string.IsNullOrWhiteSpace(config.ClientSecret)) missingKeys.Add(nameof(config.ClientSecret));
        if (string.IsNullOrWhiteSpace(config.RedirectUri)) missingKeys.Add(nameof(config.RedirectUri));
        if (string.IsNullOrWhiteSpace(config.TokenEndpoint)) missingKeys.Add(nameof(config.TokenEndpoint));
        if (string.IsNullOrWhiteSpace(config.UserInfoEndpoint)) missingKeys.Add(nameof(config.UserInfoEndpoint));
        if (string.IsNullOrWhiteSpace(config.AuthorizationEndpoint)) missingKeys.Add(nameof(config.AuthorizationEndpoint));

        if (missingKeys.Any())
        {
            logger.LogError("Missing LinkedIn OAuth configuration keys in Production: {Keys}", string.Join(", ", missingKeys));
            throw new InvalidOperationException("Missing LinkedIn OAuth configuration keys for Production");
        }
    }
}
