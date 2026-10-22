using Auth.Application.DTOs.ForgotPassword.Request;
using Auth.Application.DTOs.ForgotPassword.Response;
using Auth.Application.Ports.Outbound;
using Auth.Domain.Entities;

namespace Auth.Application.UseCases.ForgotPassword.Commands;

public sealed class ForgotPasswordCommand
{
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommand(
        IAuthenticationRepository authenticationRepository,
        IEmailService emailService)
    {
        _authenticationRepository = authenticationRepository;
        _emailService = emailService;
    }

    public async Task<ForgotPasswordResponse> ExecuteAsync(
        ForgotPasswordRequest request)
    {
        var userId =
            await _authenticationRepository
                .GetUserIdByEmailAsync(request.Email!);

        // Security
        if (userId is null)
        {
            return new ForgotPasswordResponse
            {
                Message = "If the email exists, a reset link has been sent."
            };
        }

        await _authenticationRepository
            .InvalidatePasswordResetTokensAsync(userId.Value);

        var token = Guid.NewGuid().ToString("N");

        await _authenticationRepository
            .SavePasswordResetTokenAsync(
                new PasswordResetToken
                {
                    PasswordResetTokenId = Guid.NewGuid(),
                    UserId = userId.Value,
                    Token = token,
                    ExpireAt = DateTime.UtcNow.AddMinutes(2),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                });

        var link =
            $"http://localhost:4200/#/set-password?token={token}";

        await _emailService.SendAsync(
            request.Email!,
            "Reset Password",
            $"""
            <h2>Reset Password</h2>

            <p>Click the link below to set your password.</p>

            <a href="{link}">
                Reset Password
            </a>

            <p>This link expires in 15 minutes.</p>
            """);

        return new ForgotPasswordResponse
        {
            Message = "Check your email."
        };
    }
}