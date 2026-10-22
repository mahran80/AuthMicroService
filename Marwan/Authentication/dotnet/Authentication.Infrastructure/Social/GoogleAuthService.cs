using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.Social
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ExternalUserInfo?> ValidateAsync(string credential)
        {
            if (string.IsNullOrWhiteSpace(credential))
                return null;

            var clientId = _configuration["Google:ClientId"]
                ?? _configuration["Authentication:Google:ClientId"];

            if (string.IsNullOrWhiteSpace(clientId))
            {
                _logger.LogError("Google ClientId is not configured.");
                throw new InvalidOperationException("Google ClientId is not configured.");
            }

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    credential,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    });

                if (!payload.EmailVerified)
                {
                    _logger.LogWarning("Google account email is not verified: {Email}", payload.Email);
                    return null;
                }

                return new ExternalUserInfo(
                    payload.Subject,
                    payload.Email,
                    payload.Name);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Google token validation failed: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error validating Google credential.");
                return null;
            }
        }
    }
}
