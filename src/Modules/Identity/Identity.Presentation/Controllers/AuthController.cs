using System.Security.Claims;
using Identity.Application.DTOs;
using Identity.Application.Features.ForgotPassword;
using Identity.Application.Features.Login;
using Identity.Application.Features.LoginMobile;
using Identity.Application.Features.Refresh;
using Identity.Application.Features.Register;
using Identity.Application.Features.ResendOtp;
using Identity.Application.Features.ResetPassword;
using Identity.Application.Features.Revoke;
using Identity.Application.Features.SocialLogin;
using Identity.Application.Features.VerifyOtp;
using Identity.Presentation.Contracts;
using Identity.Presentation.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedKernel.Presentation;

namespace Identity.Presentation.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IAuthCookieService _cookieService;

    public AuthController(
        ISender sender,
        IAuthCookieService cookieService)
    {
        _sender = sender;
        _cookieService = cookieService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthEmail")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.CountryCode,
            request.PhoneNumber);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails(HttpContext);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLogin")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails(HttpContext);
        }

        _cookieService.SetTokenCookies(
            Response,
            result.Value.AccessToken,
            result.Value.AccessTokenExpiresAtUtc,
            result.Value.RefreshToken,
            result.Value.RefreshTokenExpiresAtUtc);

        return Ok(result.Value);
    }

    [HttpPost("social-login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLogin")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SocialLogin(
        [FromBody] SocialLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SocialLoginCommand(request.Provider, request.Credential);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails(HttpContext);
        }

        _cookieService.SetTokenCookies(
            Response,
            result.Value.AccessToken,
            result.Value.AccessTokenExpiresAtUtc,
            result.Value.RefreshToken,
            result.Value.RefreshTokenExpiresAtUtc);

        return Ok(result.Value);
    }

    [HttpPost("login/mobile")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLogin")]
    [ProducesResponseType(typeof(OtpDispatchedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LoginMobile(
        [FromBody] LoginMobileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginMobileCommand(request.CountryCode, request.PhoneNumber);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails(HttpContext);
        }

        return Ok(result.Value);
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthSensitive")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyOtpRequest request,
        CancellationToken cancellationToken)
    {
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        string? userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var command = new VerifyOtpCommand(
            request.Email,
            request.CountryCode,
            request.PhoneNumber,
            request.Code,
            request.Purpose,
            request.DeviceName,
            request.DeviceType,
            ipAddress,
            userAgent);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails(HttpContext);
        }

        _cookieService.SetTokenCookies(
            Response,
            result.Value.AccessToken,
            result.Value.AccessTokenExpiresAtUtc,
            result.Value.RefreshToken,
            result.Value.RefreshTokenExpiresAtUtc);

        return Ok(result.Value);
    }

    [HttpPost("resend-otp")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthEmail")]
    [ProducesResponseType(typeof(OtpDispatchedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendOtp(
        [FromBody] ResendOtpRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResendOtpCommand(
            request.Email,
            request.CountryCode,
            request.PhoneNumber,
            request.Purpose);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails(HttpContext);
        }

        return Ok(result.Value);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthEmail")]
    [ProducesResponseType(typeof(PasswordResetDispatchedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(request.Email);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails(HttpContext);
        }

        return Ok(result.Value);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthSensitive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(request.Token, request.NewPassword);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails(HttpContext);
        }

        return Ok(new { message = "Password has been successfully reset." });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        string? refreshToken = request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            Request.Cookies.TryGetValue(AuthCookieService.RefreshTokenCookieName, out refreshToken);
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "Refresh token must be provided via request body or cookie.",
                Instance = HttpContext.Request.Path
            };
            problemDetails.Extensions["errorCode"] = "Identity.InvalidRefreshToken";
            return BadRequest(problemDetails);
        }

        var command = new RefreshTokenCommand(refreshToken);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails(HttpContext);
        }

        _cookieService.SetTokenCookies(
            Response,
            result.Value.AccessToken,
            result.Value.AccessTokenExpiresAtUtc,
            result.Value.RefreshToken,
            result.Value.RefreshTokenExpiresAtUtc);

        return Ok(result.Value);
    }

    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(
        [FromBody] RevokeTokenRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        string? refreshToken = request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            Request.Cookies.TryGetValue(AuthCookieService.RefreshTokenCookieName, out refreshToken);
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "Refresh token must be provided via request body or cookie.",
                Instance = HttpContext.Request.Path
            };
            problemDetails.Extensions["errorCode"] = "Identity.InvalidRefreshToken";
            return BadRequest(problemDetails);
        }

        var command = new RevokeTokenCommand(refreshToken);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails(HttpContext);
        }

        _cookieService.ClearTokenCookies(Response);

        return Ok(new { message = "Refresh token has been successfully revoked." });
    }

    [HttpGet("me")]
    [Authorize]
    [HasPermission("auth:profile:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public IActionResult GetCurrentUserProfile()
    {
        var userIdClaim = User.FindFirst("sub")?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "User identity claim could not be retrieved from the token.",
                Instance = HttpContext.Request.Path
            };
            problemDetails.Extensions["errorCode"] = "Identity.InvalidToken";
            return Unauthorized(problemDetails);
        }

        var email = User.FindFirst("email")?.Value 
            ?? User.FindFirst(ClaimTypes.Email)?.Value 
            ?? string.Empty;

        var roles = User.FindAll("roles")
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        if (roles.Count == 0)
        {
            roles = User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct()
                .ToList();
        }

        var permissions = User.FindAll("permissions")
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        return Ok(new
        {
            userId,
            email,
            roles,
            permissions
        });
    }
}
