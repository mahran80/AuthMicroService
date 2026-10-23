using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService.API.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthLoginPolicy = "AuthLogin";
    public const string AuthEmailPolicy = "AuthEmail";
    public const string AuthSensitivePolicy = "AuthSensitive";

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/problem+json";
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too Many Requests",
                    Type = "https://tools.ietf.org/html/rfc6585#section-4",
                    Detail = "Too many requests. Please try again later.",
                    Instance = context.HttpContext.Request.Path
                };

                problemDetails.Extensions["errorCode"] = "General.RateLimitExceeded";

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            };

            options.AddPolicy(AuthLoginPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(AuthEmailPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(AuthSensitivePolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
