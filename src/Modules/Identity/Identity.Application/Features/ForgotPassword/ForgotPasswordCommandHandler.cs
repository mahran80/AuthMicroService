using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Microsoft.Extensions.Configuration;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, PasswordResetDispatchedResponse>
{
    private const string DefaultResetPasswordUrl = "http://localhost:4200/#/set-password";
    private readonly IUserRepository _userRepository;
    private readonly IOtpCodeGeneratorPort _otpCodeGenerator;
    private readonly IEmailNotificationPort _emailNotificationPort;
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IOtpCodeGeneratorPort otpCodeGenerator,
        IEmailNotificationPort emailNotificationPort,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _otpCodeGenerator = otpCodeGenerator;
        _emailNotificationPort = emailNotificationPort;
        _configuration = configuration;
    }

    public async Task<Result<PasswordResetDispatchedResponse>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        // Security: Prevent account enumeration
        if (user is null)
        {
            return Result.Success(new PasswordResetDispatchedResponse(
                "If the email is registered, password reset instructions have been sent."));
        }

        string rawToken = _otpCodeGenerator.GenerateSecureUrlToken();
        string tokenHash = _otpCodeGenerator.ComputeSha256Hash(rawToken);

        var resetToken = PasswordResetToken.Create(
            user.Id,
            tokenHash,
            isInviteToken: false,
            lifetime: TimeSpan.FromMinutes(15));

        await _userRepository.AddPasswordResetTokenAsync(resetToken, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        string baseUrl = _configuration["Frontend:ResetPasswordUrl"] ?? DefaultResetPasswordUrl;
        string separator = baseUrl.Contains('?') ? "&" : "?";
        string resetLink = $"{baseUrl}{separator}token={Uri.EscapeDataString(rawToken)}";

        await _emailNotificationPort.SendPasswordResetEmailAsync(
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            resetLink,
            cancellationToken);

        return Result.Success(new PasswordResetDispatchedResponse(
            "If the email is registered, password reset instructions have been sent."));
    }
}
