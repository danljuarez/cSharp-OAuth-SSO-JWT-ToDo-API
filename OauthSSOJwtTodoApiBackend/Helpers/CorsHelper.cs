namespace OauthSSOJwtTodoApiBackend.Helpers;

public static class CorsHelper
{
    public static string AllowCorsPolicyName => "AllowCors";

    public static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Read CORS Origins setup from config
        var corsSection = configuration.GetSection("Cors");
        var devOrigins = corsSection.GetSection("AllowedDevOrigins").Get<string[]>() ?? Array.Empty<string>();
        var prodOrigins = corsSection.GetSection("AllowedProdOrigins").Get<string[]>() ?? Array.Empty<string>();

        // Choose origins based on environment
        var allowedOrigins = environment.IsDevelopment() ? devOrigins : prodOrigins;

        services.AddCors(options =>
        {
            options.AddPolicy(AllowCorsPolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
    }
}
