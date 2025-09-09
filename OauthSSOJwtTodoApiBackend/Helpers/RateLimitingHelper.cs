using OauthSSOJwtTodoApiBackend.Configuration;
using System.Threading.RateLimiting;

namespace OauthSSOJwtTodoApiBackend.Helpers;

public static class RateLimitingHelper
{
    public static void AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitConfig = configuration.GetSection("RateLimiting").Get<RateLimitSettings>()
            ?? throw new InvalidOperationException("RateLimiting config missing.");

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitConfig.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitConfig.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateLimitConfig.QueueLimit
                    }));
        });
    }
}
