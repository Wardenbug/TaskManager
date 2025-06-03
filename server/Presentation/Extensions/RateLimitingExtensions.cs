using Presentation.DTOs;
using System.Threading.RateLimiting;

namespace Presentation.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var path = httpContext.Request.Path.Value;

                if (path.StartsWith("/health") || path.StartsWith("/healthchecks-ui"))
                {
                    return RateLimitPartition.GetNoLimiter("HealthCheck");
                }
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity.Name ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        QueueLimit = 2,
                        Window = TimeSpan.FromMinutes(1)
                    });
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                   partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                   factory: partition => new FixedWindowRateLimiterOptions
                   {
                       AutoReplenishment = true,
                       PermitLimit = 10,
                       QueueLimit = 0,
                       Window = TimeSpan.FromMinutes(1)
                   });
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var response = new ErrorResponseDto(
                    "Too many requests. Please try again later.",
                    "RATE_LIMIT_EXCEEDED"
                );

                await context.HttpContext.Response.WriteAsJsonAsync(response, token);
            };
        });
        return services;
    }
}
