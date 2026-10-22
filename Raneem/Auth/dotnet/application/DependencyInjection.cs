using Auth.Application.UseCases.Login.Commands;
using Auth.Application.UseCases.Register.Commands;
using Auth.Application.UseCases.RefreshToken.Commands;
using Microsoft.Extensions.DependencyInjection;
using Auth.Application.UseCases.Otp.Commands;
using Auth.Application.UseCases.ForgotPassword.Commands;
using Auth.Application.UseCases.Logout.Commands;
using Auth.Application.UseCases.ResetPassword.Commands;
using Auth.Application.UseCases.LoginWeb.Commands;
using Auth.Application.UseCases.ChangePassword.Commands;
using Auth.Application.UseCases.Commands;
namespace Auth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {   services.AddScoped<ResetPasswordCommand>();
        services.AddScoped<ForgotPasswordCommand>();
        services.AddScoped<LoginCommand>();
        services.AddScoped<VerifyOtpCommand>();
        services.AddScoped<LogoutCommand>();
        services.AddScoped<LoginWebCommand>();
        services.AddScoped<RegisterCommand>();
        services.AddScoped<RefreshTokenCommand>();
        services.AddScoped<ResendOtpCommand>();
        services.AddScoped<ChangePasswordCommand>();
        services.AddScoped<DeleteAuthenticationCommand>();
        return services;
    }
}