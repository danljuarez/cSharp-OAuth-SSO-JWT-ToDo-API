using OauthSSOJwtTodoApiBackend.Configuration;

namespace OauthSSOJwtTodoApiBackend.Helpers;

public static class SwaggerUIHelper
{
    public static void ConfigureSwaggerUI(WebApplication app)
    {
        var configuration = app.Configuration;
        var env = app.Environment;
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        var linkedInConfig = configuration.GetSection("LinkedIn").Get<LinkedInOAuthSettings>();
        if (linkedInConfig == null)
        {
            logger.LogError("LinkedIn configuration is missing.");
            throw new InvalidOperationException("LinkedIn configuration is missing.");
        }

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "OAuth SSO JWT ToDo API V1");

            // OAuth2 redirect URI (must match LinkedIn config)
            options.OAuthClientId(linkedInConfig.ClientId);
            options.OAuthClientSecret(linkedInConfig.ClientSecret); // Optional - Needed for token exchange in Swagger
            options.OAuthUsePkce(); // Enable PKCE
            options.OAuthAppName("LinkedIn SSO Swagger");
            options.OAuthScopeSeparator(" ");
            options.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
            {
                { "response_type", "code" }
            });
        });
    }
}
