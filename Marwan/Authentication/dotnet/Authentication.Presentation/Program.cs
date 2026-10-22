using Authentication.Infrastructure;
using Authentication.Presentation;
using Authentication.Presentation.Middlewares;
using Scalar.AspNetCore;

namespace Authentication.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();

            // Register Authentication Module
            builder.Services.AddAuthenticationModule(builder.Configuration);

            // OpenAPI
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure pipeline via module extensions
            app.UseAuthenticationModule();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(); // Opens UI at http://localhost:5110/scalar/v1
            }

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}
