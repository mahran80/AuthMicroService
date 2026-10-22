// using Auth.Application.DTOs.Otp.Request;
// using Auth.Application.DTOs.Otp.Response;
// using Auth.Application.Ports.Outbound;
// using Auth.Domain.Entities;

// namespace Auth.Application.UseCases.Otp.Commands;

// public sealed class VerifyOtpCommand
// {
//     private readonly IAuthenticationRepository _authenticationRepository;
//     private readonly IJwtService _jwtService;

//     public VerifyOtpCommand(
//         IAuthenticationRepository authenticationRepository,
//         IJwtService jwtService)
//     {
//         _authenticationRepository = authenticationRepository;
//         _jwtService = jwtService;
//     }

//     public async Task<VerifyOtpResponse> ExecuteAsync(
//         OtpCommandRequest request)
//     {
//         Guid? userId;

//         if (!string.IsNullOrWhiteSpace(request.Email))
//         {
//             userId =
//                 await _authenticationRepository.GetUserIdByEmailAsync(
//                     request.Email);
//         }
//         else
//         {
//             userId =
//                 await _authenticationRepository.GetUserIdByPhoneAsync(
//                     request.CountryCode!,
//                     request.PhoneNumber!);
//         }

//         if (userId is null)
//         {
//             return new VerifyOtpResponse
//             {
//                 Success = false,
//                 Message = "User not found."
//             };
//         }

//         var otpTypeId =
//             await _authenticationRepository.GetOtpTypeIdAsync(
//                 request.CommandType);
// Console.WriteLine("========== VERIFY OTP ==========");
// Console.WriteLine($"CommandType = {request.CommandType}");
// Console.WriteLine($"CountryCode = {request.CountryCode}");
// Console.WriteLine($"PhoneNumber = {request.PhoneNumber}");
// Console.WriteLine($"Code = {request.Code}");
// Console.WriteLine($"UserId = {userId}");
//         var otp =
//             await _authenticationRepository.GetOtpAsync(
//                 userId.Value,
//                 request.Code,
//                 otpTypeId);

// Console.WriteLine($"OtpTypeId = {otpTypeId}");
//         if (otp is null)
//         {
//             return new VerifyOtpResponse
//             {
//                 Success = false,
//                 Message = "Invalid OTP"
//             };
//         }

//         if (otp.ExpireAt <= DateTime.UtcNow)
//         {
//             return new VerifyOtpResponse
//             {
//                 Success = false,
//                 Message = "OTP expired"
//             };
//         }

//         otp.IsUsed = true;

//         if (request.CommandType.Equals(
//                 "Register",
//                 StringComparison.OrdinalIgnoreCase))
//         {
//             await _authenticationRepository.VerifyContactAsync(
//                 userId.Value);
//         }

//         await _authenticationRepository.UpdateOtpAsync(otp);

//         var accessToken =
//             _jwtService.GenerateAccessToken(userId.Value);

//         var refreshToken =
//             _jwtService.GenerateRefreshToken();

//         var refreshTokenEntity = new  Auth.Domain.Entities.RefreshToken
//         {
//             RefreshTokenId = Guid.NewGuid(),
//             UserId = userId.Value,
//             Token = refreshToken,
//             ExpireAt = _jwtService.GetRefreshTokenExpiration(),
//             IsRevoked = false,
//             RevokedAt = null
//         };

//         await _authenticationRepository.SaveRefreshTokenAsync(
//             refreshTokenEntity);

//         return new VerifyOtpResponse
//         {
//             Success = true,
//             Message = request.CommandType.Equals(
//                 "Register",
//                 StringComparison.OrdinalIgnoreCase)
//                 ? "Registration completed successfully."
//                 : "Login successful.",

//             AccessToken = accessToken,

//             AccessTokenExpiresAt =
//                 _jwtService.GetAccessTokenExpiration(),

//             RefreshToken = refreshToken,

//             RefreshTokenExpiresAt =
//                 refreshTokenEntity.ExpireAt
//         };
//     }
// }
using Auth.Application.DTOs.Otp.Request;
using Auth.Application.DTOs.Otp.Response;
using Auth.Application.Ports.Outbound;

namespace Auth.Application.UseCases.Otp.Commands;

public sealed class VerifyOtpCommand
{
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IJwtService _jwtService;

 private readonly IHrGrpcClient _hrGrpcClient;

public VerifyOtpCommand(
    IAuthenticationRepository authenticationRepository,
    IJwtService jwtService,
    IHrGrpcClient hrGrpcClient
)
{
    _authenticationRepository = authenticationRepository;
    _jwtService = jwtService;
     _hrGrpcClient = hrGrpcClient;
}


    public async Task<VerifyOtpResponse> ExecuteAsync(
        OtpCommandRequest request)
    {

        // =====================================================
        // REGISTER VERIFICATION
        // =====================================================

        if (request.CommandType.Equals(
                "Register",
                StringComparison.OrdinalIgnoreCase))
        {

            var pendingRegistration =
                await _authenticationRepository
                    .GetPendingRegistrationAsync(
                        request.Email,
                        request.CountryCode,
                        request.PhoneNumber,
                        request.Code!);



            if (pendingRegistration is null)
            {
                return new VerifyOtpResponse
                {
                    Success = false,
                    Message = "Invalid OTP"
                };
            }



            if (pendingRegistration.ExpireAt <= DateTime.UtcNow)
            {
                return new VerifyOtpResponse
                {
                    Success = false,
                    Message = "OTP expired"
                };
            }



            var userId = Guid.NewGuid();



            await _authenticationRepository.RegisterAsync(
                userId,
                pendingRegistration.Name,
                pendingRegistration.Email,
                pendingRegistration.CountryCode,
                pendingRegistration.PhoneNumber,
                pendingRegistration.PasswordHash);

            await _hrGrpcClient.CreatePassengerAsync(userId);

            await _authenticationRepository.VerifyContactAsync(
                userId);

            // =====================================
            // Save Verified OTP History
            // IAM_Authentication_OTP
            // =====================================

            await _authenticationRepository.SaveOtpAsync(
                new Auth.Domain.Entities.Otp
                {
                    OtpId = Guid.NewGuid(),

                    UserId = userId,

                    Code = request.Code!,

                    TypeId =
                        await _authenticationRepository
                            .GetOtpTypeIdAsync("Register"),

                    ExpireAt =
                        pendingRegistration.ExpireAt,

                    IsUsed = true,

                    CanResetPassword = false
                });


            await _authenticationRepository
                .DeletePendingRegistrationAsync(
                    pendingRegistration);


// var passengerId =
//     await _hrGrpcClient.CreatePassengerAsync(userId);
            return await GenerateTokensAsync(
                userId,
                "Registration completed successfully."
                    // passengerId: passengerId
                );
        }



        // =====================================================
        // LOGIN VERIFICATION
        // =====================================================


        Guid? loginUserId;



        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            loginUserId =
                await _authenticationRepository
                    .GetUserIdByEmailAsync(
                        request.Email);
        }
        else
        {
            loginUserId =
                await _authenticationRepository
                    .GetUserIdByPhoneAsync(
                        request.CountryCode!,
                        request.PhoneNumber!);
        }



        if (loginUserId is null)
        {
            return new VerifyOtpResponse
            {
                Success = false,
                Message = "User not found."
            };
        }



        var otpTypeId =
            await _authenticationRepository
                .GetOtpTypeIdAsync(
                    "Login");



        var otp =
            await _authenticationRepository
                .GetOtpAsync(
                    loginUserId.Value,
                    request.Code!,
                    otpTypeId);



        if (otp is null)
        {
            return new VerifyOtpResponse
            {
                Success = false,
                Message = "Invalid OTP"
            };
        }



        if (otp.ExpireAt <= DateTime.UtcNow)
        {
            return new VerifyOtpResponse
            {
                Success = false,
                Message = "OTP expired"
            };
        }



        otp.IsUsed = true;



        await _authenticationRepository
            .UpdateOtpAsync(
                otp);
        Console.WriteLine("Before SaveOtpAsync");
        await _authenticationRepository.SaveOtpAsync(

            new Auth.Domain.Entities.Otp
            {
                OtpId = Guid.NewGuid(),

                UserId = loginUserId.Value,

                Code = request.Code!,

                TypeId = otpTypeId,

                ProjectType = otp.ProjectType,

                ExpireAt = DateTime.UtcNow,

                IsUsed = true,

                CanResetPassword = otp.CanResetPassword
            });
        // Console.WriteLine("After SaveOtpAsync");
        return await GenerateTokensAsync(
            loginUserId.Value,
            "Login successful.");
    }





    // =====================================================
    // TOKEN GENERATION
    // =====================================================

    private async Task<VerifyOtpResponse> GenerateTokensAsync(
    Guid userId,
    string message)
    {
        var name =
            await _authenticationRepository.GetUserNameAsync(userId);

        var accessToken =
            _jwtService.GenerateAccessToken(userId);

        var refreshToken =
            _jwtService.GenerateRefreshToken();

        var refreshTokenEntity =
            new Auth.Domain.Entities.RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = userId,
                Token = refreshToken,
                ExpireAt = _jwtService.GetRefreshTokenExpiration(),
                IsRevoked = false
            };

        await _authenticationRepository
            .SaveRefreshTokenAsync(refreshTokenEntity);

        return new VerifyOtpResponse
        {
            Success = true,
            Message = message,
            UserId = userId,
            Name = name,
            AccessToken = accessToken,
            AccessTokenExpiresAt =
                _jwtService.GetAccessTokenExpiration(),
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt =
                refreshTokenEntity.ExpireAt
        };
    }
}