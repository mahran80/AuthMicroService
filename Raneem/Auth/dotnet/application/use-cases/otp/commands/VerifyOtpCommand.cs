// using Auth.Application.DTOs.Otp.Request;
// using Auth.Application.DTOs.Otp.Response;
// using Auth.Application.Ports.Outbound;

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
//         VerifyOtpRequest request)
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

//         // Get OTP
// // في VerifyOtp ده خاص بتفعيل التسجيل
// var registerTypeId =
//     await _authenticationRepository.GetOtpTypeIdAsync("Register");

// var otp =
//     await _authenticationRepository.GetOtpAsync(
//         userId.Value,
//         request.Code,
//         registerTypeId);

//         if (otp is null)
//         {
//             return new VerifyOtpResponse
//             {
//                 Success = false,
//                 Message = "Invalid OTP"
//             };
//         }

//         // Check Expiration
//         if (otp.ExpireAt <= DateTime.UtcNow)
//         {
//             return new VerifyOtpResponse
//             {
//                 Success = false,
//                 Message = "OTP expired"
//             };
//         }

//         // Mark OTP as Used
// // Get OTP Types
// // var registerTypeId =
// //     await _authenticationRepository.GetOtpTypeIdAsync("Register");



// // Mark OTP as Used
// otp.IsUsed = true;

// // Register OTP
// if (otp.TypeId == registerTypeId)
// {
//     await _authenticationRepository.VerifyContactAsync(
//         userId.Value);
// }

// // Forgot Password OTP

// // Save Changes
// await _authenticationRepository.UpdateOtpAsync(otp);

//         // Generate Tokens
//         var accessToken =
//             _jwtService.GenerateAccessToken(userId.Value);

//         var refreshToken =
//             _jwtService.GenerateRefreshToken();

//         var refreshTokenEntity = new Auth.Domain.Entities.RefreshToken
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
// Console.WriteLine("=================================");
// Console.WriteLine("Refresh Token Saved:");
// Console.WriteLine(refreshTokenEntity.Token);
// Console.WriteLine("=================================");
//         return new VerifyOtpResponse
//         {
//             Success = true,
//             Message = "OTP verified successfully.",

//             AccessToken = accessToken,
//             AccessTokenExpiresAt =
//                 _jwtService.GetAccessTokenExpiration(),

//             RefreshToken = refreshToken,
//             RefreshTokenExpiresAt =
//                 refreshTokenEntity.ExpireAt
//         };
//     }
// }