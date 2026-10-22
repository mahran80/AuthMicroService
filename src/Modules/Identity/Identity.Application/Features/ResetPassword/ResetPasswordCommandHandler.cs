using Identity.Application.Abstractions.Ports;
using Identity.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.ResetPassword;

public sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpCodeGeneratorPort _otpCodeGenerator;
    private readonly IPasswordHasherPort _passwordHasher;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IOtpCodeGeneratorPort otpCodeGenerator,
        IPasswordHasherPort passwordHasher)
    {
        _userRepository = userRepository;
        _otpCodeGenerator = otpCodeGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        string tokenHash = _otpCodeGenerator.ComputeSha256Hash(request.Token.Trim());

        var resetToken = await _userRepository.GetActivePasswordResetTokenByHashAsync(tokenHash, cancellationToken);
        if (resetToken is null || !resetToken.IsValid)
        {
            return Result.Failure(IdentityErrors.InvalidOrExpiredResetToken);
        }

        var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(IdentityErrors.UserNotFound);
        }

        string newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatePasswordHash(newPasswordHash);
        resetToken.MarkAsUsed();

        // Invalidate all active refresh tokens for the user upon password reset
        await _userRepository.RevokeAllActiveUserTokensAsync(user.Id, "PasswordReset", cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
