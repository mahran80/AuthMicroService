using Authentication.Application.Configuration;
using Authentication.Application.DTOs;
using Authentication.Application.Exceptions;
using Authentication.Application.Interfaces;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Authentication.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IOtpHasher _otpHasher;
        private readonly IEmailVerificationOtpRepository _otpRepository;
        private readonly FrontendSettings _frontendSettings;
        private readonly IAuthenticationCookieService _cookieService;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IFacebookAuthService _facebookAuthService;
        private readonly ILinkedInAuthService _linkedInAuthService;
        private readonly IGitHubAuthService _gitHubAuthService;
        private readonly IMicrosoftAuthService _microsoftAuthService;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IOtpHasher otpHasher,
            IEmailVerificationOtpRepository otpRepository,
            IOptions<FrontendSettings> frontendSettings,
            IAuthenticationCookieService cookieService,
            IAccessTokenService accessTokenService,
            IRefreshTokenService refreshTokenService,
            IGoogleAuthService googleAuthService,
            IFacebookAuthService facebookAuthService,
            ILinkedInAuthService linkedInAuthService,
            IGitHubAuthService gitHubAuthService,
            IMicrosoftAuthService microsoftAuthService
            )
        {
            _userManager = userManager;
            _emailService = emailService;
            _otpHasher = otpHasher;
            _otpRepository = otpRepository;
            _frontendSettings = frontendSettings.Value;
            _cookieService = cookieService;
            _accessTokenService = accessTokenService;
            _refreshTokenService = refreshTokenService;
            _googleAuthService = googleAuthService;
            _facebookAuthService = facebookAuthService;
            _linkedInAuthService = linkedInAuthService;
            _gitHubAuthService = gitHubAuthService;
            _microsoftAuthService = microsoftAuthService;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser is not null)
            {
                if (existingUser.EmailConfirmed)
                {
                    throw new ConflictException("An account with this email already exists.");
                }

                throw new ConflictException("This email is already registered but has not been verified. Please use the resend verification option.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Status = UserStatus.Pending
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                throw new BadRequestException(errors);
            }

            // Generate Identity email confirmation token and URL-safe encode it
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));

            // Generate 6-digit OTP
            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var otpHash = _otpHasher.Hash(otp);

            var now = DateTime.UtcNow;

            var verificationOtp = new EmailVerificationOtp
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CodeHash = otpHash,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(5),
                FailedAttempts = 0
            };

            await _otpRepository.AddAsync(verificationOtp);
            await _otpRepository.SaveChangesAsync();

            var verificationLink = $"{_frontendSettings.BaseUrl}{_frontendSettings.EmailVerificationPath}" +
                                   $"?userId={Uri.EscapeDataString(user.Id.ToString())}&token={encodedToken}";

            var emailBody = BuildVerificationEmailBody(verificationLink, otp);

            try
            {
                await _emailService.SendEmailAsync(user.Email!, "Verify your ifikra account", emailBody);
            }
            catch (Exception ex)
            {
                // Rollback: delete created user to avoid orphaned unverified record
                await _userManager.DeleteAsync(user);
                throw new EmailServiceException("Failed to send verification email. Please try again.", ex);
            }
        }

        public async Task ResendVerificationAsync(ResendVerificationRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
                throw new NotFoundException("No account found with this email address.");

            if (user.EmailConfirmed)
                throw new ConflictException("This email address is already verified.");

            // Invalidate previously active OTPs.
            await _otpRepository.InvalidateOtpsForUserAsync(user.Id);

            // Generate a new email confirmation token.
            var confirmationToken =
                await _userManager
                    .GenerateEmailConfirmationTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));

            var otp = RandomNumberGenerator
                    .GetInt32(10000, 100000)
                    .ToString();

            var otpHash = _otpHasher.Hash(otp);

            var now = DateTime.UtcNow;

            var verificationOtp = new EmailVerificationOtp
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CodeHash = otpHash,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(5),
                FailedAttempts = 0
            };

            await _otpRepository.AddAsync(verificationOtp);

            await _otpRepository.SaveChangesAsync();

            var verificationLink =
                $"{_frontendSettings.BaseUrl}" +
                $"{_frontendSettings.EmailVerificationPath}" +
                $"?userId={Uri.EscapeDataString(user.Id.ToString())}" +
                $"&token={Uri.EscapeDataString(encodedToken)}";

            var emailBody = BuildVerificationEmailBody(verificationLink, otp);

            try
            {
                await _emailService.SendEmailAsync(
                    user.Email!,
                    "Verify your account",
                    emailBody);
            }
            catch (Exception ex)
            {
                throw new EmailServiceException(
                    "Failed to send verification email. " +
                    "Please try again.",
                    ex);
            }
        }

        public async Task VerifyEmailAsync(VerifyEmailRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
            {
                throw new NotFoundException("User not found.");
            }

            if (user.EmailConfirmed)
            {
                throw new ConflictException("Email is already verified.");
            }

            string decodedToken;
            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
                decodedToken = Encoding.UTF8.GetString(tokenBytes);
            }
            catch
            {
                throw new BadRequestException("Invalid verification token format.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                throw new BadRequestException("Invalid or expired verification link.");
            }

            user.Status = UserStatus.Active;
            await _userManager.UpdateAsync(user);

            // Invalidate pending OTPs
            await _otpRepository.InvalidateOtpsForUserAsync(user.Id);
            await _otpRepository.SaveChangesAsync();

            // Auto-login upon successful verification
            await IssueAuthenticationCookiesAsync(user);
        }

        public async Task VerifyOtpAsync(VerifyOtpRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                throw new NotFoundException("Invalid email or verification code.");
            }

            if (user.EmailConfirmed)
            {
                throw new ConflictException("Email is already verified.");
            }

            var otp = await _otpRepository.GetActiveOtpByUserIdAsync(user.Id);

            if (otp is null)
            {
                throw new BadRequestException("Verification code is invalid or expired.");
            }

            if (otp.FailedAttempts >= 5)
            {
                throw new BadRequestException("Too many failed attempts. Please request a new code.");
            }

            var isValid = _otpHasher.Verify(request.Otp, otp.CodeHash);

            if (!isValid)
            {
                otp.FailedAttempts++;

                if (otp.FailedAttempts >= 5)
                {
                    otp.UsedAt = DateTime.UtcNow;
                }

                await _otpRepository.SaveChangesAsync();
                throw new BadRequestException("Invalid verification code.");
            }

            otp.UsedAt = DateTime.UtcNow;

            user.EmailConfirmed = true;
            user.Status = UserStatus.Active;

            await _userManager.UpdateAsync(user);
            await _otpRepository.SaveChangesAsync();

            // Auto-login upon successful verification
            await IssueAuthenticationCookiesAsync(user);
        }

        public async Task LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(
                request.Email);

            if (user is null)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (!user.EmailConfirmed)
            {
                throw new ForbiddenException("Please verify your email before logging in.");
            }

            if (user.Status != UserStatus.Active)
            {
                throw new ForbiddenException("Your account is not active.");
            }

            var passwordValid =
                await _userManager.CheckPasswordAsync(
                    user,
                    request.Password);

            if (!passwordValid)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            await IssueAuthenticationCookiesAsync(user);
        }

        public async Task SocialLoginAsync(SocialLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Provider))
            {
                throw new BadRequestException("Provider is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Credential))
            {
                throw new BadRequestException("Credential is required.");
            }

            var providerLower = request.Provider.Trim().ToLowerInvariant();
            var loginProvider = providerLower switch
            {
                "google" => "Google",
                "facebook" => "Facebook",
                "linkedin" =>"Linkedin",
                "github" =>"GitHub",
                "microsoft"=>"Microsoft",
                _ => throw new BadRequestException($"Unsupported provider '{request.Provider}'.")
            };

            ExternalUserInfo? externalUser = providerLower switch
            {
                "google" => await _googleAuthService.ValidateAsync(request.Credential),
                "facebook" => await _facebookAuthService.ValidateAsync(request.Credential),
                "linkedin"=>await _linkedInAuthService.ValidateAsync(request.Credential),
                "github"=>await _gitHubAuthService.ValidateAsync(request.Credential),
                "microsoft"=>await _microsoftAuthService.ValidateAsync(request.Credential),
                _ => null
            };

            if (externalUser is null)
            {
                throw new UnauthorizedException($"Invalid {loginProvider} credentials.");
            }

            var user = await _userManager.FindByLoginAsync(loginProvider, externalUser.ProviderUserId);

            if (user is null)
            {
                user = await _userManager.FindByEmailAsync(externalUser.Email);

                if (user is null)
                {
                    var (firstName, lastName) = SplitFullName(externalUser.Name);

                    user = new ApplicationUser
                    {
                        UserName = externalUser.Email,
                        Email = externalUser.Email,
                        FirstName = firstName,
                        LastName = lastName,
                        Status = UserStatus.Active,
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        throw new BadRequestException(errors);
                    }
                }
                else
                {
                    if (!user.EmailConfirmed)
                    {
                        user.EmailConfirmed = true;
                        await _userManager.UpdateAsync(user);
                    }
                }

                var addLoginResult = await _userManager.AddLoginAsync(
                    user,
                    new UserLoginInfo(loginProvider, externalUser.ProviderUserId, loginProvider));

                if (!addLoginResult.Succeeded)
                {
                    var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                    throw new BadRequestException(errors);
                }
            }

            if (user.Status != UserStatus.Active)
            {
                throw new ForbiddenException("Your account is not active.");
            }

            await IssueAuthenticationCookiesAsync(user);
        }

        private async Task IssueAuthenticationCookiesAsync(ApplicationUser user)
        {
            var (accessToken, accessTokenExpiresAt) =
                await _accessTokenService.GenerateAsync(user);

            var (refreshToken, refreshTokenExpiresAt) =
                await _refreshTokenService.CreateAsync(user);

            _cookieService.SetAccessToken(
                accessToken,
                accessTokenExpiresAt);

            _cookieService.SetRefreshToken(
                refreshToken,
                refreshTokenExpiresAt);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null || !user.EmailConfirmed)
                return;


            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken =WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            var resetLink =
                $"{_frontendSettings.BaseUrl}" +
                $"{_frontendSettings.PasswordResetPath}" +
                $"?userId={Uri.EscapeDataString(user.Id.ToString())}" +
                $"&token={Uri.EscapeDataString(encodedToken)}";

            var emailBody = BuildPasswordResetEmailBody(resetLink);

            try
            {
                await _emailService.SendEmailAsync(
                    user.Email!,
                    "Reset your password",
                    emailBody);
            }
            catch (Exception ex)
            {
                throw new EmailServiceException(
                    "Failed to send password reset email. " +
                    "Please try again.",
                    ex);
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
                throw new NotFoundException("User account was not found.");

            if (!user.EmailConfirmed)
                throw new ConflictException("Email address must be verified before resetting the password.");
            try
            {

                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

                var result = await _userManager.ResetPasswordAsync(
                        user,
                        decodedToken,
                        request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        " ",
                        result.Errors.Select(x => x.Description));

                    throw new ValidationException(errors);
                }
                // logout from all devices
                await _refreshTokenService.RevokeAllAsync(user.Id);
            }
            catch
            {
                throw new ValidationException("Invalid or expired password reset token.");
            }
        }
            
        //helpers
        private static string BuildVerificationEmailBody(string verificationLink, string otp)
        {
            return $"""
            <h2>Welcome to ifikra app!</h2>
            <p>Thank you for creating your account. Please verify your email address using one of the options below.</p>
            <p><a href="{verificationLink}">Verify Email</a></p>
            <p>Or enter this verification code:</p>
            <h2>{otp}</h2>
            <p>This verification code will expire in 5 minutes.</p>
            <p>If you did not create this account, you can safely ignore this email.</p>
            """;
        }

        private string BuildPasswordResetEmailBody(string resetLink)
        {
            return $"""
        <h2>Reset Your Password</h2>

        <p>
            We received a request to reset your password.
        </p>

        <p>
            Click the link below to choose a new password:
        </p>

        <p>
            <a href="{resetLink}">
                Reset Password
            </a>
        </p>

        <p>
            If you did not request a password reset,
            you can safely ignore this email.
        </p>

        <p>
            This link is valid for a limited time.
        </p>
        """;
        }

        private static (string FirstName, string LastName) SplitFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return ("User", string.Empty);
            }

            var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                1 => (parts[0], string.Empty),
                _ => (parts[0], parts[1])
            };
        }
    }
}   
