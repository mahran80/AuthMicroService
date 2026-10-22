using Auth.Application.DTOs.ResetPassword.Request;
using Auth.Application.DTOs.ResetPassword.Response;
using Auth.Application.Ports.Outbound;

namespace Auth.Application.UseCases.ResetPassword.Commands;

public sealed class ResetPasswordCommand
{
    private readonly IAuthenticationRepository _authenticationRepository;

    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommand(
        IAuthenticationRepository authenticationRepository,
        IPasswordHasher passwordHasher)
    {
        _authenticationRepository = authenticationRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ResetPasswordResponse> ExecuteAsync(
        ResetPasswordRequest request)
    {
        var resetToken =
            await _authenticationRepository
                .GetPasswordResetTokenAsync(request.Token);

        if (resetToken is null)
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "Invalid reset link."
            };
        }

        if (resetToken.IsUsed)
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "This link has already been used."
            };
        }

        if (resetToken.ExpireAt <= DateTime.UtcNow)
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "Reset link expired."
            };
        }

        var passwordHash =
            _passwordHasher.Hash(request.NewPassword);

        await _authenticationRepository.ResetPasswordAsync(
            resetToken.UserId,
            passwordHash);

        resetToken.IsUsed = true;

        await _authenticationRepository
            .UpdatePasswordResetTokenAsync(resetToken);

        return new ResetPasswordResponse
        {
            Success = true,
            Message = "Password reset successfully."
        };
    }
}