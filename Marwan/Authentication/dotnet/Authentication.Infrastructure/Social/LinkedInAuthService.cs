
using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.Social
{
    public class LinkedInAuthService : ILinkedInAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LinkedInAuthService> _logger;
        private readonly IConfiguration _configuration;

        public LinkedInAuthService(HttpClient httpClient,ILogger<LinkedInAuthService> logger,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<string?> ExchangeCodeForTokenAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("LinkedIn authorization code is empty.");
                return null;
            }
            var clientId = _configuration["LinkedInSettings:ClientId"];
            var clientSecret = _configuration["LinkedInSettings:ClientSecret"];
            var redirectUri = _configuration["LinkedInSettings:RedirectUri"];
            if(string.IsNullOrEmpty(redirectUri)||string.IsNullOrEmpty(clientId)||string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogWarning("LinkedIn OAuth configuration is incomplete.");
                return null;
            }
            var request=new HttpRequestMessage(HttpMethod.Post, "https://www.linkedin.com/oauth/v2/accessToken") 
            {
                    Content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["grant_type"] = "authorization_code",
                        ["code"] = code,
                        ["redirect_uri"] = redirectUri,
                        ["client_id"] = clientId,
                        ["client_secret"] = clientSecret
                    })
            };
            var response= await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("LinkedIn token endpoint returned {StatusCode}.",response.StatusCode);
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<LinkedInTokenResponse>();
            if (string.IsNullOrWhiteSpace(result.AccessToken))
            {
                _logger.LogWarning("LinkedIn did not return an access token.");
                return null;
            }
            return result.AccessToken;
        }
        public async Task<LinkedInUser> GetUserAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogWarning("LinkedIn access token is empty.");
                return null;
            }
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/userinfo");
            request.Headers.Authorization =new AuthenticationHeaderValue("Bearer", accessToken);

            var response=await _httpClient.SendAsync(request);
            if(!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("LinkedIn user request returned {StatusCode}");
                return null ;
            }
            var user=await response.Content.ReadFromJsonAsync<LinkedInUser>();
            if(user is null)
            {
                _logger.LogError("LinkedIn returned an invalid user profile.");
                return null;
            }
            return user;
        }
        public async Task<ExternalUserInfo?> ValidateAsync(string credential)
        {
            if (!string.IsNullOrWhiteSpace(credential))
            {
                _logger.LogWarning("LinkedIn authorization code is empty.");
                return null;
            }
            var accesstoken=await ExchangeCodeForTokenAsync(credential);
            if (string.IsNullOrWhiteSpace(accesstoken))
            {
                return null;
            }
            var user=await GetUserAsync(accesstoken);
            if (user is null)
                return null;

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning(
                    "LinkedIn user {Subject} has no available email.",
                    user.Sub);

                return null;
            }

            return new ExternalUserInfo(
                user.Sub!,
                user.Email,
                user.Name ?? string.Empty);
        }
        private sealed class LinkedInTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }

        public sealed class LinkedInUser
        {
            [JsonPropertyName("sub")]
            public string? Sub { get; set; }

            [JsonPropertyName("email")]
            public string? Email { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }
    }
}
