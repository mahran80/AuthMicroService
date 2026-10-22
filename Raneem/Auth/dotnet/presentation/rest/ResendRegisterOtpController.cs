using Auth.Application.DTOs.Otp.Request;
using Auth.Application.UseCases.Otp.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class VerifyOtpController : ControllerBase
{
    private readonly VerifyOtpCommand _verifyOtpCommand;
    private readonly ResendOtpCommand _resendOtpCommand;

    public VerifyOtpController(
        VerifyOtpCommand verifyOtpCommand,
        ResendOtpCommand resendOtpCommand)
    {
        _verifyOtpCommand = verifyOtpCommand;
        _resendOtpCommand = resendOtpCommand;
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(
        OtpCommandRequest request)
    {
        var result =
            await _verifyOtpCommand.ExecuteAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        Response.Cookies.Append(
            "refreshToken",
            result.RefreshToken!,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.RefreshTokenExpiresAt
            });

        return Ok(new
        {   result.Name,
            result.UserId,
            result.Success,
            result.Message,
            result.AccessToken,
            result.AccessTokenExpiresAt
        });
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(
        OtpCommandRequest request)
    {
        var result =
            await _resendOtpCommand.ExecuteAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}