using Microsoft.Extensions.DependencyInjection;
using Users.Application.UseCases.Commands;
using Users.Application.UseCases.Queries;


namespace Users.Application;

public static class DependencyInjection
{
public static IServiceCollection AddUsersApplication(
    this IServiceCollection services)
    {
    services.AddScoped<CreateUserCommand>();
    services.AddScoped<UpdateUserCommand>();
    services.AddScoped<DeleteUserCommand>();
    services.AddScoped<GetUserByPhoneQuery>();
        return services;
    }
}