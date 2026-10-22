// using Auth.Application.DTOs.Otp.Request;
// using Auth.Application.DTOs.Otp.Response;
// using Auth.Application.Ports.Outbound;
// using Notifications.WhatsApp.Application.Dtos;

// namespace Auth.Application.UseCases.Otp.Commands;

// public sealed class ResendOtpCommand
// {
//     private readonly IAuthenticationRepository _repository;
//     private readonly IOtpService _otpService;
//     private readonly IWhatsAppService _whatsAppService;

//     public ResendOtpCommand(
//         IAuthenticationRepository repository,
//         IOtpService otpService,
//         IWhatsAppService whatsAppService)
//     {
//         _repository = repository;
//         _otpService = otpService;
//         _whatsAppService = whatsAppService;
//     }

//     public async Task<ResendOtpResponse> ExecuteAsync(
//         OtpCommandRequest request)
//     {
//         Guid? userId;

//         if (!string.IsNullOrWhiteSpace(request.Email))
//         {
//             userId = await _repository.GetUserIdByEmailAsync(
//                 request.Email);
//         }
//         else
//         {
//             userId = await _repository.GetUserIdByPhoneAsync(
//                 request.CountryCode!,
//                 request.PhoneNumber!);
//         }

//         if (userId is null)
//         {
//             return new ResendOtpResponse
//             {
//                 Success = false,
//                 Message = "User not found."
//             };
//         }

//         var otpTypeId =
//             await _repository.GetOtpTypeIdAsync(
//                 request.CommandType);

//         await _repository.InvalidateActiveOtpsAsync(
//             userId.Value,
//             otpTypeId);

//         var otpCode =
//             _otpService.GenerateOtp();

//         var otp = new Auth.Domain.Entities.Otp
//         {
//             OtpId = Guid.NewGuid(),
//             UserId = userId.Value,
//             Code = otpCode,
//             TypeId = otpTypeId,
//             ProjectType = request.ProjectType,
//             ExpireAt = _otpService.GetOtpExpiration(),
//             IsUsed = false,
//             CanResetPassword = false
//         };

//         await _repository.SaveOtpAsync(otp);

//         var sendResult =
//             await _whatsAppService.SendTextMessageAsync(
//                 new WhatsAppMessageRequest
//                 {
//                     To = $"{request.CountryCode}{request.PhoneNumber}",
//                     Body = $"Your Lady Driver verification code is: {otpCode}"
//                 });

//         if (!sendResult.IsSuccess)
//         {
//             return new ResendOtpResponse
//             {
//                 Success = false,
//                 Message = sendResult.ErrorMessage
//             };
//         }

//         return new ResendOtpResponse
//         {
//             Success = true,
//             Message = "OTP sent successfully."
//         };
//     }
// }
using Auth.Application.DTOs.Otp.Request;
using Auth.Application.DTOs.Otp.Response;
using Auth.Application.Ports.Outbound;
using Notifications.WhatsApp.Application.Dtos;

namespace Auth.Application.UseCases.Otp.Commands;

public sealed class ResendOtpCommand
{
    private readonly IAuthenticationRepository _repository;
    private readonly IOtpService _otpService;
    private readonly IWhatsAppService _whatsAppService;

    public ResendOtpCommand(
        IAuthenticationRepository repository,
        IOtpService otpService,
        IWhatsAppService whatsAppService)
    {
        _repository = repository;
        _otpService = otpService;
        _whatsAppService = whatsAppService;
    }

    public async Task<ResendOtpResponse> ExecuteAsync(
        OtpCommandRequest request)
    {
        // =====================================================
        // REGISTER
        // =====================================================
        if (request.CommandType.Equals(
                "Register",
                StringComparison.OrdinalIgnoreCase))
        {
            var pendingRegistration =
                await _repository.GetPendingRegistrationByPhoneAsync(
                    request.CountryCode!,
                    request.PhoneNumber!);

            if (pendingRegistration is null)
            {
                return new ResendOtpResponse
                {
                    Success = false,
                    Message = "Registration request not found."
                };
            }

            var otpCode = _otpService.GenerateOtp();

            pendingRegistration.OtpCode = otpCode;
            pendingRegistration.ExpireAt =
                _otpService.GetOtpExpiration();

            await _repository.UpdatePendingRegistrationAsync(
                pendingRegistration);

            var sendResult =
                await _whatsAppService.SendTextMessageAsync(
                    new WhatsAppMessageRequest
                    {
                        To = $"{request.CountryCode}{request.PhoneNumber}",
                        Body =
                            $"Your Lady Driver verification code is: {otpCode}"
                    });

            if (!sendResult.IsSuccess)
            {
                return new ResendOtpResponse
                {
                    Success = false,
                    Message = sendResult.ErrorMessage
                };
            }

            return new ResendOtpResponse
            {
                Success = true,
                Message = "OTP sent successfully."
            };
        }

        // =====================================================
        // LOGIN
        // =====================================================

        Guid? userId;

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            userId = await _repository.GetUserIdByEmailAsync(
                request.Email);
        }
        else
        {
            userId = await _repository.GetUserIdByPhoneAsync(
                request.CountryCode!,
                request.PhoneNumber!);
        }

        if (userId is null)
        {
            return new ResendOtpResponse
            {
                Success = false,
                Message = "User not found."
            };
        }

        var otpTypeId =
            await _repository.GetOtpTypeIdAsync(
                request.CommandType);

        await _repository.InvalidateActiveOtpsAsync(
            userId.Value,
            otpTypeId);

        var loginOtp = new Auth.Domain.Entities.Otp
        {
            OtpId = Guid.NewGuid(),
            UserId = userId.Value,
            Code = _otpService.GenerateOtp(),
            TypeId = otpTypeId,
            ProjectType = request.ProjectType,
            ExpireAt = _otpService.GetOtpExpiration(),
            IsUsed = false,
            CanResetPassword = false
        };

        await _repository.SaveOtpAsync(loginOtp);

        var loginSendResult =
            await _whatsAppService.SendTextMessageAsync(
                new WhatsAppMessageRequest
                {
                    To = $"{request.CountryCode}{request.PhoneNumber}",
                    Body =
                        $"Your Lady Driver verification code is: {loginOtp.Code}"
                });

        if (!loginSendResult.IsSuccess)
        {
            return new ResendOtpResponse
            {
                Success = false,
                Message = loginSendResult.ErrorMessage
            };
        }

        return new ResendOtpResponse
        {
            Success = true,
            Message = "OTP sent successfully."
        };
    }
}