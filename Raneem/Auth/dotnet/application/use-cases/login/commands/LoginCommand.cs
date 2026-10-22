using Auth.Application.DTOs.Login.Request;
using Auth.Application.DTOs.Login.Response;
using Auth.Application.Ports.Outbound;
using Auth.Domain.Entities;
using Notifications.WhatsApp.Application.Dtos;

namespace Auth.Application.UseCases.Login.Commands;

public sealed class LoginCommand
{
    private readonly IAuthenticationRepository _repository;
    private readonly IOtpService _otpService;
    private readonly IWhatsAppService _whatsAppService;

    public LoginCommand(
        IAuthenticationRepository repository,
        IOtpService otpService,
        IWhatsAppService whatsAppService)
    {
        _repository = repository;
        _otpService = otpService;
        _whatsAppService = whatsAppService;
    }

    public async Task<LoginResponse> ExecuteAsync(LoginRequest request)
{
    await using var transaction =
        await _repository.BeginTransactionAsync();

    try
    {
        var user =
            await _repository.GetByPhoneAsync(
                request.CountryCode,
                request.PhoneNumber);

        if (user is null)
            throw new Exception("Phone number not found.");

        var otpTypeId =
            await _repository.GetOtpTypeIdAsync("Login");

        await _repository.InvalidateActiveOtpsAsync(
            user.UserId,
            otpTypeId);

        var otpCode =
            _otpService.GenerateOtp();

        var otp = new Auth.Domain.Entities.Otp
        {
            OtpId = Guid.NewGuid(),
            UserId = user.UserId,
            Code = otpCode,
            TypeId = otpTypeId,
            ProjectType = request.ProjectType,
            ExpireAt = _otpService.GetOtpExpiration(),
            IsUsed = false,
            CanResetPassword = false
        };

        await _repository.SaveOtpAsync(otp);

        var sendResult =
            await _whatsAppService.SendTextMessageAsync(
                new WhatsAppMessageRequest
                {
                    To = $"{request.CountryCode}{request.PhoneNumber}",
                    Body = $"Your Lady Driver verification code is: {otpCode}"
                });

        if (!sendResult.IsSuccess)
            throw new Exception(sendResult.ErrorMessage);

        await transaction.CommitAsync();

        return new LoginResponse
        {
            Message = "OTP sent successfully.",
            OtpCode = otp.Code
        };
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
}