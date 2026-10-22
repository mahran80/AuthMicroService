using Auth.Application.DTOs.ChangePassword.Request;
using Auth.Application.DTOs.ChangePassword.Response;
using Auth.Application.Ports.Outbound;

namespace Auth.Application.UseCases.ChangePassword.Commands;

public sealed class ChangePasswordCommand
{
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommand(
        IAuthenticationRepository authenticationRepository,
        IPasswordHasher passwordHasher)
    {
        _authenticationRepository = authenticationRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ChangePasswordResponse> ExecuteAsync(
        ChangePasswordRequest request)
    {
        var setPasswordToken =
            await _authenticationRepository
                .GetSetPasswordTokenAsync(request.Token);

        if (setPasswordToken is not null)
        {
            return await HandleSetPasswordAsync(
                setPasswordToken,
                request.NewPassword);
        }

        var resetPasswordToken =
            await _authenticationRepository
                .GetPasswordResetTokenAsync(request.Token);

        if (resetPasswordToken is not null)
        {
            return await HandleResetPasswordAsync(
                resetPasswordToken,
                request.NewPassword);
        }

        return new ChangePasswordResponse
        {
            Success = false,
            Message = "Invalid token."
        };
    }

    // =====================================================
    // SET PASSWORD
    // =====================================================

    private async Task<ChangePasswordResponse> HandleSetPasswordAsync(
        Auth.Domain.Entities.SetPasswordToken token,
        string newPassword)
    {
        if (token.IsUsed)
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = "Token has already been used."
            };
        }

        if (token.ExpireAt <= DateTime.UtcNow)
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = "Token has expired."
            };
        }

        var hasPassword =
            await _authenticationRepository
                .HasActivePasswordAsync(token.UserId);

        if (hasPassword)
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = "Password is already set."
            };
        }

        var passwordHash =
            _passwordHasher.Hash(newPassword);

        await _authenticationRepository.SetPasswordAsync(
            token.UserId,
            passwordHash);

        token.IsUsed = true;

        await _authenticationRepository
            .UpdateSetPasswordTokenAsync(token);

        return new ChangePasswordResponse
        {
            Success = true,
            Message = "Password has been set successfully."
        };
    }

    // =====================================================
    // RESET PASSWORD
    // =====================================================

    private async Task<ChangePasswordResponse> HandleResetPasswordAsync(
        Auth.Domain.Entities.PasswordResetToken token,
        string newPassword)
    {
        if (token.IsUsed)
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = "This link has already been used."
            };
        }

        if (token.ExpireAt <= DateTime.UtcNow)
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = "Reset link expired."
            };
        }

        var passwordHash =
            _passwordHasher.Hash(newPassword);

        await _authenticationRepository.ResetPasswordAsync(
            token.UserId,
            passwordHash);

        token.IsUsed = true;

        await _authenticationRepository
            .UpdatePasswordResetTokenAsync(token);

        return new ChangePasswordResponse
        {
            Success = true,
            Message = "Password reset successfully."
        };
    }
}