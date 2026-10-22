using Authentication.Infrastructure;
using Authentication.Presentation.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Presentation
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuthenticationModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Exception Handling
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            // Infrastructure (which also registers Application services)
            services.AddInfrastructureServices(configuration);

            return services;
        }

        public static IApplicationBuilder UseAuthenticationModule(this IApplicationBuilder app)
        {
            app.UseExceptionHandler();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}
