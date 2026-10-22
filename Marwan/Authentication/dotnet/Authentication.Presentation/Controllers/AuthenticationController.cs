using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Authentication.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Authentication.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController(IAuthenticationService _authenticationService,
        IRefreshTokenService _refreshTokenService,
        IAccessTokenService _accessTokenService,
        IAuthenticationCookieService _cookieService
        )
        : ControllerBase
    {
        [HttpPost("register")]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            await _authenticationService.RegisterAsync(request);

            return Ok(new
            {
                message = "Registration successful. Please check your email to verify your account."
            });
        }

        [HttpPost("resend-verification")]
        [EnableRateLimiting("AuthEmail")]
        public async Task<IActionResult> ResendVerification(ResendVerificationRequest request)
        {
            await _authenticationService.ResendVerificationAsync(request);

            return Ok(new
            {
                message = "Verification instructions have been resent. Please check your email."
            });
        }

        [HttpPost("verify-email")]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
        {
            await _authenticationService.VerifyEmailAsync(request);

            return Ok(new
            {
                message = "Email verified successfully."
            });
        }

        [HttpPost("verify-otp")]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
        {
            await _authenticationService.VerifyOtpAsync(request);

            return Ok(new
            {
                message = "Email verified successfully."
            });
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthLogin")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            await _authenticationService.LoginAsync(request);

            return Ok(new
            {
                message = "Login successful."
            });
        }

        [HttpPost("social-login")]
        [EnableRateLimiting("AuthLogin")]
        public async Task<IActionResult> SocialLogin(SocialLoginRequest request)
        {
            await _authenticationService.SocialLoginAsync(request);

            return Ok(new
            {
                message = "Login successful."
            });
        }
    
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier),

                email = User.FindFirstValue(
                    JwtRegisteredClaimNames.Email),

                userName = User.FindFirstValue(
                    ClaimTypes.Name)
            });
        }

        [HttpPost("refresh")]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken =Request.Cookies[AuthenticationCookieNames.RefreshToken];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new
                {
                    message = "Refresh token is missing."
                });
            }

            var storedRefreshToken = await _refreshTokenService.ValidateAsync(refreshToken);

            if (storedRefreshToken is null)
            {
                _cookieService.ClearAuthenticationCookies();

                return Unauthorized(new
                {
                    message = "Invalid or expired refresh token."
                });
            }

            var (newRefreshToken, refreshTokenExpiresAt) =
                await _refreshTokenService.RotateAsync(
                    storedRefreshToken);

            var (accessToken, accessTokenExpiresAt) =
                await _accessTokenService.GenerateAsync(
                    storedRefreshToken.User);

            _cookieService.SetAccessToken( accessToken,accessTokenExpiresAt);

            _cookieService.SetRefreshToken( newRefreshToken, refreshTokenExpiresAt);

            return Ok(new
            {
                message = "Token refreshed successfully."
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken =Request.Cookies[AuthenticationCookieNames.RefreshToken];

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var storedRefreshToken =await _refreshTokenService.ValidateAsync(refreshToken);

                if (storedRefreshToken is not null)
                    await _refreshTokenService.RevokeAsync(storedRefreshToken);
            }

            _cookieService.ClearAuthenticationCookies();

            return Ok(new
            {
                message = "Logged out successfully."
            });
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting("AuthEmail")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            await _authenticationService.ForgotPasswordAsync(request);

            return Ok(new
            {
                message ="If an account exists with this email, " +
                    "a password reset link has been sent."
            });
        }

        [HttpPost("reset-password")]
        [EnableRateLimiting("AuthSensitive")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            await _authenticationService.ResetPasswordAsync(
                request);

            return Ok(new
            {
                message = "Password reset successfully."
            });
        }

        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userId, out var parsedUserId))
                return Unauthorized();


            await _refreshTokenService.RevokeAllAsync(parsedUserId);

            _cookieService.ClearAuthenticationCookies();

            return Ok(new
            {
                message = "Logged out from all devices successfully."
            });
        }
    }
}
