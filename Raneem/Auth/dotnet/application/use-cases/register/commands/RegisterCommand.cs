// using Auth.Application.DTOs.Register.Request;
// using Auth.Application.DTOs.Register.Response;
// using Auth.Application.Ports.Outbound;
// using Auth.Domain.Entities;
// using Notifications.WhatsApp.Application.Dtos;
// namespace Auth.Application.UseCases.Register.Commands;

// public sealed class RegisterCommand
// {
//     private readonly IAuthenticationRepository _authenticationRepository;
//     private readonly IPasswordHasher _passwordHasher;
//     private readonly IOtpService _otpService;
//     private readonly IWhatsAppService _whatsAppService;

// public RegisterCommand(
//     IAuthenticationRepository authenticationRepository,
//     IPasswordHasher passwordHasher,
//     IOtpService otpService,
//     IWhatsAppService whatsAppService)
// {
//     _authenticationRepository = authenticationRepository;
//     _passwordHasher = passwordHasher;
//     _otpService = otpService;
//     _whatsAppService = whatsAppService;
// }

//    public async Task<RegisterResponse> ExecuteAsync(RegisterRequest request)
// {
//     await using var transaction =
//         await _authenticationRepository.BeginTransactionAsync();

//     try
//     {
//         var userId = Guid.NewGuid();

//         string? passwordHash = null;

//         // ============================
//         // Register by Email
//         // ============================
//         if (!string.IsNullOrWhiteSpace(request.Email))
//         {
//             var existingEmail =
//                 await _authenticationRepository.GetByEmailAsync(
//                     request.Email);

//             if (existingEmail is not null)
//                 throw new Exception("Email already exists.");

//             if (string.IsNullOrWhiteSpace(request.Password))
//                 throw new Exception("Password is required.");

//             passwordHash =
//                 _passwordHasher.Hash(request.Password);
//         }
//         else if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
//         {
//             if (string.IsNullOrWhiteSpace(request.CountryCode))
//                 throw new Exception("Country code is required.");

//             var existingPhone =
//                 await _authenticationRepository.GetUserIdByPhoneAsync(
//                     request.CountryCode,
//                     request.PhoneNumber);

//             if (existingPhone is not null)
//                 throw new Exception("Phone number already exists.");
//         }
//         else
//         {
//             throw new Exception("Email or Phone Number is required.");
//         }

//         await _authenticationRepository.RegisterAsync(
//             userId,
//             request.Name,
//             request.Email,
//             request.CountryCode,
//             request.PhoneNumber,
//             passwordHash);

//         var otpCode =
//             _otpService.GenerateOtp();

//         var otpTypeId =
//             await _authenticationRepository.GetOtpTypeIdAsync("Register");

//         var otp = new Auth.Domain.Entities.Otp
//         {
//             OtpId = Guid.NewGuid(),
//             UserId = userId,
//             Code = otpCode,
//             TypeId = otpTypeId,
//             ProjectType = request.ProjectType,
//             ExpireAt = _otpService.GetOtpExpiration(),
//             IsUsed = false,
//             CanResetPassword = false
//         };

//         await _authenticationRepository.SaveOtpAsync(otp);

//         var sendResult =
//             await _whatsAppService.SendTextMessageAsync(
//                 new WhatsAppMessageRequest
//                 {
//                     To = $"{request.CountryCode}{request.PhoneNumber}",
//                     Body = $"Your Lady Driver verification code is: {otpCode}"
//                 });

//         if (!sendResult.IsSuccess)
//             throw new Exception(sendResult.ErrorMessage);

//         await transaction.CommitAsync();

//         return new RegisterResponse
//         {
//             Message = "OTP sent successfully."
//         };
//     }
//     catch
//     {
//         await transaction.RollbackAsync();
//         throw;
//     }
// }
// }
using Auth.Application.DTOs.Register.Request;
using Auth.Application.DTOs.Register.Response;
using Auth.Application.Ports.Outbound;
using Auth.Domain.Entities;
using Notifications.WhatsApp.Application.Dtos;

namespace Auth.Application.UseCases.Register.Commands;

public sealed class RegisterCommand
{
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpService _otpService;
    private readonly IWhatsAppService _whatsAppService;

    public RegisterCommand(
        IAuthenticationRepository authenticationRepository,
        IPasswordHasher passwordHasher,
        IOtpService otpService,
        IWhatsAppService whatsAppService)
    {
        _authenticationRepository = authenticationRepository;
        _passwordHasher = passwordHasher;
        _otpService = otpService;
        _whatsAppService = whatsAppService;
    }


    public async Task<RegisterResponse> ExecuteAsync(
        RegisterRequest request)
    {
        await using var transaction =
            await _authenticationRepository.BeginTransactionAsync();

        try
        {
            string? passwordHash = null;


            // ============================
            // Register By Email
            // ============================
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existingEmail =
                    await _authenticationRepository.GetByEmailAsync(
                        request.Email);


                if (existingEmail is not null)
                    throw new Exception(
                        "Email already exists.");


                if (string.IsNullOrWhiteSpace(request.Password))
                    throw new Exception(
                        "Password is required.");


                passwordHash =
                    _passwordHasher.Hash(
                        request.Password);
            }


            // ============================
            // Register By Phone
            // ============================
            else if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                if (string.IsNullOrWhiteSpace(request.CountryCode))
                    throw new Exception(
                        "Country code is required.");


                var existingPhone =
                    await _authenticationRepository
                        .GetUserIdByPhoneAsync(
                            request.CountryCode,
                            request.PhoneNumber);


                if (existingPhone is not null)
                    throw new Exception(
                        "Phone number already exists.");
            }

            else
            {
                throw new Exception(
                    "Email or Phone Number is required.");
            }



            // ============================
            // Generate OTP
            // ============================
            var otpCode =
                _otpService.GenerateOtp();



            // ============================
            // Check Existing Pending Registration
            // ============================

            PendingRegistration? pendingRegistration = null;


            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                pendingRegistration =
                    await _authenticationRepository
                        .GetPendingRegistrationByPhoneAsync(
                            request.CountryCode!,
                            request.PhoneNumber);
            }


            // ============================
            // Update Existing Pending
            // ============================

            if (pendingRegistration is not null)
            {
                pendingRegistration.Name =
                    request.Name;

                pendingRegistration.Email =
                    request.Email;

                pendingRegistration.PasswordHash =
                    passwordHash;

                pendingRegistration.ProjectType =
                    request.ProjectType;

                pendingRegistration.OtpCode =
                    otpCode;

                pendingRegistration.ExpireAt =
                    _otpService.GetOtpExpiration();

                await _authenticationRepository
                    .UpdatePendingRegistrationAsync(
                        pendingRegistration);
            }


            // ============================
            // Create New Pending
            // ============================

            else
            {
                pendingRegistration =
                    new PendingRegistration
                    {
                        PendingRegistrationId =
                            Guid.NewGuid(),

                        Name =
                            request.Name,

                        Email =
                            request.Email,

                        CountryCode =
                            request.CountryCode,

                        PhoneNumber =
                            request.PhoneNumber,

                        PasswordHash =
                            passwordHash,

                        ProjectType =
                            request.ProjectType,

                        OtpCode =
                            otpCode,

                        ExpireAt =
                            _otpService.GetOtpExpiration(),

                        CreatedAt =
                            DateTime.UtcNow
                    };


                await _authenticationRepository
                    .SavePendingRegistrationAsync(
                        pendingRegistration);
            }



            // ============================
            // Send OTP
            // ============================

            var sendResult =
                await _whatsAppService.SendTextMessageAsync(
                    new WhatsAppMessageRequest
                    {
                        To =
                            $"{request.CountryCode}{request.PhoneNumber}",

                        Body =
                            $"Your Lady Driver verification code is: {otpCode}"
                    });


            if (!sendResult.IsSuccess)
            {
                throw new Exception(
                    sendResult.ErrorMessage);
            }


            await transaction.CommitAsync();


            return new RegisterResponse
            {    
                Message =
                    "OTP sent successfully.",
                        OtpCode = otpCode
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}