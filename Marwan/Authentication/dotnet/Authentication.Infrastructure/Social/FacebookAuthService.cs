using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.Social
{
    public sealed class FacebookAuthService : IFacebookAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FacebookAuthService> _logger;

        public FacebookAuthService(HttpClient httpClient, ILogger<FacebookAuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ExternalUserInfo?> ValidateAsync(string credential)
        {
            if (string.IsNullOrWhiteSpace(credential))
            {
                _logger.LogWarning("Facebook credential is empty.");
                return null;
            }

            try
            {
                var url = $"https://graph.facebook.com/me?fields=id,name,email&access_token={Uri.EscapeDataString(credential)}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Facebook validation failed with status {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                    return null;
                }

                var facebookUser = await response.Content.ReadFromJsonAsync<FacebookUserResponse>(
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (facebookUser is null || string.IsNullOrWhiteSpace(facebookUser.Id))
                {
                    _logger.LogWarning("Facebook returned an invalid or empty user profile.");
                    return null;
                }

                return new ExternalUserInfo(
                    facebookUser.Id,
                    facebookUser.Email ?? string.Empty,
                    facebookUser.Name ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error validating Facebook credential.");
                return null;
            }
        }

        private sealed class FacebookUserResponse
        {
            public string Id { get; set; } = string.Empty;
            public string? Name { get; set; }
            public string? Email { get; set; }
        }
    }
}
