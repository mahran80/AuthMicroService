using Identity.Application.Abstractions.Ports;
using Identity.Infrastructure.Integration;
using Identity.Infrastructure.Notifications;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Contracts;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? configuration.GetConnectionString("Database")
                ?? "Server=(localdb)\\mssqllocaldb;Database=AuthMicroserviceDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "identity");
            });
        });

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<UltraMsgOptions>(configuration.GetSection(UltraMsgOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasherPort, BcryptPasswordHasherAdapter>();
        services.AddSingleton<IJwtProviderPort, JwtProviderAdapter>();
        services.AddSingleton<IOtpCodeGeneratorPort, OtpCodeGeneratorAdapter>();
        services.AddScoped<IUserIdentityPort, UserIdentityAdapter>();

        services.AddHttpClient<IWhatsAppNotificationPort, UltraMsgWhatsAppAdapter>();
        services.AddScoped<IEmailNotificationPort, SmtpEmailAdapter>();

        return services;
    }
}