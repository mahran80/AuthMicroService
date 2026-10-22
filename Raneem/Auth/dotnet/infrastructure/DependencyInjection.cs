using Auth.Application.Ports.Outbound;
using Auth.Application.Ports.Outbound.web;
using Auth.Infrastructure.Persistence.Context;
using Auth.Infrastructure.Persistence.Repositories;
using Auth.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Auth.Application.UseCases.Login.Commands;
using Auth.Application.UseCases.Otp.Commands;
using Notifications.WhatsApp.Infrastructure.Configuration;
using Auth.Infrastructure.Configuration;
using Auth.Infrastructure.Services;
using Notifications.WhatsApp.Infrastructure.Services;
using HrGrpc;
namespace Auth.Infrastructure;

public static class DependencyInjection
{
public static IServiceCollection AddAuthInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {    services.AddScoped<LoginCommand>();
        services.AddDbContext<AuthenticationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IJwtServiceWeb, JwtServiceWeb>();
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
services.Configure<EmailOptions>(
    configuration.GetSection("Email"));

services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
services.AddScoped<ResendOtpCommand>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddOptions<UltraMsgOptions>()
            .Bind(configuration.GetSection(UltraMsgOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.InstanceId), "UltraMsg:InstanceId is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Token), "UltraMsg:Token is required")
            .ValidateOnStart();

        services.AddHttpClient<IWhatsAppService, UltraMsgWhatsAppService>((sp, client) =>
        {
            var options = configuration
                .GetSection(UltraMsgOptions.SectionName)
                .Get<UltraMsgOptions>()!;

            client.BaseAddress =
                new Uri($"{options.BaseUrl.TrimEnd('/')}/{options.InstanceId}/");

            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddGrpcClient<HrService.HrServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:7034");
});
services.AddScoped<IHrGrpcClient, HrGrpcClient>();
        return services;
    }
}

