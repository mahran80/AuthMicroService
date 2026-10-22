// // using Auth.Application.DTOs.Otp.Request;
// // using Auth.Application.UseCases.Otp.Commands;
// // using Microsoft.AspNetCore.Mvc;

// // namespace Auth.Presentation.Controllers;

// // [ApiController]
// // [Route("api/auth")]
// // public sealed class VerifyRegisterOtpController : ControllerBase
// // {
// //     private readonly VerifyOtpCommand _verifyOtpCommand;

// //     public VerifyRegisterOtpController(
// //         VerifyOtpCommand verifyOtpCommand)
// //     {
// //         _verifyOtpCommand = verifyOtpCommand;
// //     }

// //     [HttpPost("verify-register-otp")]
// //     public async Task<IActionResult> VerifyOtp(
// //         VerifyOtpRequest request)
// //     {
// //         var result =
// //             await _verifyOtpCommand.ExecuteAsync(request);

// //         if (!result.Success)
// //             return BadRequest(result);

// //         return Ok(result);
// //     }
// // }
// using Auth.Application.DTOs.Otp.Request;
// using Auth.Application.UseCases.Otp.Commands;
// using Microsoft.AspNetCore.Mvc;

// namespace Auth.Presentation.Controllers;

// [ApiController]
// [Route("api/auth")]
// public sealed class VerifyRegisterOtpController : ControllerBase
// {
//     private readonly VerifyOtpCommand _verifyOtpCommand;

//     public VerifyRegisterOtpController(
//         VerifyOtpCommand verifyOtpCommand)
//     {
//         _verifyOtpCommand = verifyOtpCommand;
//     }

//     [HttpPost("verify-register-otp")]
//     public async Task<IActionResult> VerifyOtp(
//         VerifyOtpRequest request)
//     {
//         var result =
//             await _verifyOtpCommand.ExecuteAsync(request);

//         if (!result.Success)
//             return BadRequest(result);

//         Response.Cookies.Append(
//             "refreshToken",
//             result.RefreshToken!,
//             new CookieOptions
//             {
//                 HttpOnly = true,
//                 Secure = true,
//                 SameSite = SameSiteMode.None,
//                 Expires = result.RefreshTokenExpiresAt
//             });

//         return Ok(new
//         {
//             result.Success,
//             result.Message,
//             result.AccessToken,
//             result.AccessTokenExpiresAt
//         });
//     }
// }