using Authentication.Application.Interfaces;
using Authentication.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            return services;
        }
    }
}
