// // using Auth.Application.DTOs.VerifyLoginOtp.Request;
// // using Auth.Application.UseCases.Login.Commands;
// // using Microsoft.AspNetCore.Mvc;

// // namespace Auth.Presentation.Controllers;

// // [ApiController]
// // [Route("api/auth")]
// // public sealed class VerifyLoginOtpController : ControllerBase
// // {
// //     private readonly VerifyLoginOtpCommand _verifyLoginOtpCommand;

// //     public VerifyLoginOtpController(
// //         VerifyLoginOtpCommand verifyloginOtpCommand)
// //     {
// //         _verifyLoginOtpCommand = verifyloginOtpCommand;
// //     }

// //     [HttpPost("verify-login-otp")]
// //     public async Task<IActionResult> VerifyLoginOtp(
// //         VerifyLoginOtpRequest request)
// //     {
// //         var result = await _verifyLoginOtpCommand.ExecuteAsync(request);

// //         return Ok(result);
// //     }
// // }
// using Auth.Application.DTOs.VerifyLoginOtp.Request;
// using Auth.Application.UseCases.Login.Commands;
// using Microsoft.AspNetCore.Mvc;

// namespace Auth.Presentation.Controllers;

// [ApiController]
// [Route("api/auth")]
// public sealed class VerifyLoginOtpController : ControllerBase
// {
//     private readonly VerifyLoginOtpCommand _verifyLoginOtpCommand;

//     public VerifyLoginOtpController(
//         VerifyLoginOtpCommand verifyLoginOtpCommand)
//     {
//         _verifyLoginOtpCommand = verifyLoginOtpCommand;
//     }

//     [HttpPost("verify-login-otp")]
//     public async Task<IActionResult> VerifyLoginOtp(
//         VerifyLoginOtpRequest request)
//     {
//         var result =
//             await _verifyLoginOtpCommand.ExecuteAsync(request);

//         if (!result.Success)
//             return BadRequest(result);

//         Response.Cookies.Append(
//             "refreshToken",
//             result.RefreshToken,
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