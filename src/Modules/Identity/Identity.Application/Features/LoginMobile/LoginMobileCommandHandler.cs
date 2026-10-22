using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.LoginMobile;

public sealed class LoginMobileCommandHandler : ICommandHandler<LoginMobileCommand, OtpDispatchedResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpCodeGeneratorPort _otpCodeGenerator;
    private readonly IWhatsAppNotificationPort _whatsAppNotificationPort;

    public LoginMobileCommandHandler(
        IUserRepository userRepository,
        IOtpCodeGeneratorPort otpCodeGenerator,
        IWhatsAppNotificationPort whatsAppNotificationPort)
    {
        _userRepository = userRepository;
        _otpCodeGenerator = otpCodeGenerator;
        _whatsAppNotificationPort = whatsAppNotificationPort;
    }

    public async Task<Result<OtpDispatchedResponse>> Handle(
        LoginMobileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByPhoneAsync(request.CountryCode, request.PhoneNumber, cancellationToken);
        if (user is null)
        {
            return Result.Failure<OtpDispatchedResponse>(IdentityErrors.PhoneNotFound);
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<OtpDispatchedResponse>(IdentityErrors.AccountNotActive);
        }

        string otpCode = _otpCodeGenerator.GenerateSixDigitCode();
        string codeHash = _otpCodeGenerator.ComputeSha256Hash(otpCode);

        var otp = OtpVerification.Create(
            user.Id,
            request.CountryCode,
            request.PhoneNumber,
            user.Email,
            codeHash,
            OtpPurpose.MobileLogin,
            TimeSpan.FromMinutes(5));

        await _userRepository.AddOtpVerificationAsync(otp, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _whatsAppNotificationPort.SendOtpMessageAsync(
            request.CountryCode,
            request.PhoneNumber,
            otpCode,
            cancellationToken);

        return Result.Success(new OtpDispatchedResponse(
            "Verification code sent via WhatsApp.",
            300));
    }
}
