using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Ports.Outbound;
using Users.Infrastructure.Persistence.Context;
using Users.Infrastructure.Persistence.Repositories;
using Users.Infrastructure.Configuration;
using Users.Infrastructure.Services;
using Users.Application.UseCases.Commands;
using Users.Application.UseCases.Queries;
namespace Users.Infrastructure;

public static class DependencyInjection
{
public static IServiceCollection AddUsersInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));
        });
services.Configure<EmailOptions>(
    configuration.GetSection("Email"));
services.AddScoped<IAuthGrpcClient, AuthGrpcClient>();
services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<CreateUserCommand>();
    services.AddScoped<UpdateUserCommand>();
    services.AddScoped<GetUserByIdQuery>();

        return services;
    }
}