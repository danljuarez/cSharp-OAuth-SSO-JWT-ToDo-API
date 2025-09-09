using Microsoft.OpenApi.Models;
using OauthSSOJwtTodoApiBackend.Configuration;

namespace OauthSSOJwtTodoApiBackend.Helpers;

public static class SwaggerHelper
{
    public static void AddSwaggerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var linkedInConfig = configuration.GetSection("LinkedIn").Get<LinkedInOAuthSettings>()
            ?? throw new InvalidOperationException("LinkedIn configuration is missing.");

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "OAuth SSO JWT ToDo API",
                Version = "v1"
            });

            // === JWT Bearer Token ===
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer <token>'"
            });

            // === OAuth2 - LinkedIn ===
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(linkedInConfig.AuthorizationEndpoint),
                        TokenUrl = new Uri(linkedInConfig.TokenEndpoint),
                        Scopes = new Dictionary<string, string>
                            {
                                { "r_liteprofile", "Basic LinkedIn profile" },
                                { "r_emailaddress", "Email address" }
                            }
                    }
                },
                In = ParameterLocation.Header,
                Name = "Authorization"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        // JWT Bearer requirement
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        Array.Empty<string>()
                    },

                    {
                        // LinkedIn OAuth2 requirement
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "oauth2",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new[] { "r_liteprofile", "r_emailaddress" }
                    }
                });
        });
    }
}
