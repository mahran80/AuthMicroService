using AccessControl.Application.Abstractions.Ports;
using AccessControl.Infrastructure.Integration;
using AccessControl.Infrastructure.Persistence;
using AccessControl.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Contracts;

namespace AccessControl.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAccessControlInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AccessControlDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? configuration.GetConnectionString("Database")
                ?? "Server=(localdb)\\mssqllocaldb;Database=AuthMicroserviceDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IRbacRepository, RbacRepository>();
        services.AddScoped<IRbacAccessPort, RbacAccessAdapter>();

        return services;
    }
}
