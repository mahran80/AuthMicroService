using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.ResendOtp;

public sealed class ResendOtpCommandHandler : ICommandHandler<ResendOtpCommand, OtpDispatchedResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpCodeGeneratorPort _otpCodeGenerator;
    private readonly IWhatsAppNotificationPort _whatsAppNotificationPort;
    private readonly IEmailNotificationPort _emailNotificationPort;

    public ResendOtpCommandHandler(
        IUserRepository userRepository,
        IOtpCodeGeneratorPort otpCodeGenerator,
        IWhatsAppNotificationPort whatsAppNotificationPort,
        IEmailNotificationPort emailNotificationPort)
    {
        _userRepository = userRepository;
        _otpCodeGenerator = otpCodeGenerator;
        _whatsAppNotificationPort = whatsAppNotificationPort;
        _emailNotificationPort = emailNotificationPort;
    }

    public async Task<Result<OtpDispatchedResponse>> Handle(
        ResendOtpCommand request,
        CancellationToken cancellationToken)
    {
        User? user = null;

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.CountryCode) && !string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            user = await _userRepository.GetByPhoneAsync(
                request.CountryCode.Trim(),
                request.PhoneNumber.Trim(),
                cancellationToken);
        }

        if (user is null)
        {
            return Result.Failure<OtpDispatchedResponse>(IdentityErrors.UserNotFound);
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<OtpDispatchedResponse>(IdentityErrors.AccountNotActive);
        }

        if (!Enum.TryParse<OtpPurpose>(request.Purpose, true, out var purpose))
        {
            return Result.Failure<OtpDispatchedResponse>(IdentityErrors.InvalidOrExpiredOtp);
        }

        string otpCode = _otpCodeGenerator.GenerateSixDigitCode();
        string codeHash = _otpCodeGenerator.ComputeSha256Hash(otpCode);

        string? countryCode = !string.IsNullOrWhiteSpace(request.CountryCode) ? request.CountryCode : user.CountryCode;
        string? phoneNumber = !string.IsNullOrWhiteSpace(request.PhoneNumber) ? request.PhoneNumber : user.PhoneNumber;

        var otp = OtpVerification.Create(
            user.Id,
            countryCode,
            phoneNumber,
            user.Email,
            codeHash,
            purpose,
            TimeSpan.FromMinutes(5));

        await _userRepository.AddOtpVerificationAsync(otp, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Dispatch via WhatsApp if phone available, else Email
        if (!string.IsNullOrWhiteSpace(countryCode) && !string.IsNullOrWhiteSpace(phoneNumber))
        {
            await _whatsAppNotificationPort.SendOtpMessageAsync(
                countryCode,
                phoneNumber,
                otpCode,
                cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(user.Email))
        {
            await _emailNotificationPort.SendEmailOtpAsync(
                user.Email,
                $"{user.FirstName} {user.LastName}".Trim(),
                otpCode,
                cancellationToken);
        }

        return Result.Success(new OtpDispatchedResponse(
            "Verification code resent successfully.",
            300));
    }
}
