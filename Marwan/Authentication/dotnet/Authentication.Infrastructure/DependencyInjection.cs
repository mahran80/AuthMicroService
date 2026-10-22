using Authentication.Application;
using Authentication.Application.Configuration;
using Authentication.Application.Interfaces;
using Authentication.Domain.Entities;
using Authentication.Domain.Repositories;
using Authentication.Infrastructure.Auth;
using Authentication.Infrastructure.Email;
using Authentication.Infrastructure.Persistence.Context;
using Authentication.Infrastructure.Persistence.Repositories;
using Authentication.Infrastructure.Security;
using Authentication.Infrastructure.Social;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.RateLimiting;

namespace Authentication.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Application Services
            services.AddApplicationServices();

            // Register DbContext
            services.AddDbContext<IamDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsHistoryTable(
                            "__EFMigrationsHistory",
                            "iam");
                    });
            });

            // Register Identity
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddRoles<IdentityRole<Guid>>()
                .AddSignInManager<SignInManager<ApplicationUser>>()
                .AddEntityFrameworkStores<IamDbContext>()
                .AddDefaultTokenProviders();

            // Configure JWT
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.ConfigureOptions<JwtBearerOptionsSetup>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer();

            // Email Configuration
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // Frontend Setting Configuration
            services.Configure<FrontendSettings>(configuration.GetSection("FrontendSettings"));

            // Repositories Registration
            services.AddScoped<IEmailVerificationOtpRepository, EmailVerificationOtpRepository>();

            // Services Registration
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IOtpHasher, OtpHasher>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddHttpClient<IFacebookAuthService, FacebookAuthService>();
            services.AddHttpClient<ILinkedInAuthService, LinkedInAuthService>();
            services.AddHttpClient<IGitHubAuthService, GitHubAuthService>();
            services.AddHttpClient<IMicrosoftAuthService, MicrosoftAuthService>();
            services.AddHttpContextAccessor();
            services.AddScoped<IAuthenticationCookieService, AuthenticationCookieService>();
            services.AddScoped<IAccessTokenService, AccessTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            // Rate Limiter Registration
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";

                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new
                        {
                            message = "Too many requests. Please try again later."
                        },
                        cancellationToken);
                };

                options.AddPolicy("AuthLogin", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                options.AddPolicy("AuthSensitive", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                options.AddPolicy("AuthEmail", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
            });

            return services;
        }
    }
}
