using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using static Authentication.Infrastructure.Social.GitHubAuthService;

namespace Authentication.Infrastructure.Social
{
    public class MicrosoftAuthService:IMicrosoftAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MicrosoftAuthService> _logger;
        private readonly IConfiguration _configuration;
        public MicrosoftAuthService(HttpClient httpClient,ILogger<MicrosoftAuthService>logger,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<string?> ExchangeCodeForTokenAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogError("code is essential");
                return null;
            }
            var clientId = _configuration["Microsoft:ClientId"];
            var clientSecret = _configuration["Microsoft:ClientSecret"];
            var tenantId = _configuration["Microsoft:TenantId"];
            var redirectUri = _configuration["Microsoft:RedirectUri"];
            if(string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri)||string.IsNullOrEmpty(tenantId)) 
            {
                _logger.LogError("Microsoft OAuth configuration is incomplete.");
                return null;
            }
            var tokenUrl=$"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["code"] =code ,
                    ["redirect_uri"] = redirectUri,
                    ["grant_type"] = "authorization_code",
                    ["scope"] = "openid profile email User.Read"
                })
            };

            var response = await _httpClient.SendAsync(request);
            if(!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Microsoft token exchange failed with status {StatusCode}", response.StatusCode);
                return null;
            }
            var result =await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>();

            if (string.IsNullOrWhiteSpace(result?.AccessToken))
            {
                _logger.LogWarning(
                    "Microsoft token response did not contain an access token.");

                return null;
            }
            return result.AccessToken;
        }
        private async Task<MicrosoftUser?> GetUserAsync(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me?$select=id,displayName,mail,userPrincipalName");
            var response= await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var user = await response.Content.ReadFromJsonAsync<MicrosoftUser>();

            if (user is null)
                return null;

            return new MicrosoftUser
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Mail = user.Mail,
                UserPrincipalName = user.UserPrincipalName
            };
        }
        public async Task<ExternalUserInfo?>ValidateAsync(string credential) 
        {
            var accessToken=await ExchangeCodeForTokenAsync(credential);
            if (accessToken is null)
                return null;
            var user = await GetUserAsync(accessToken);

            if (user is null || string.IsNullOrWhiteSpace(user.Id))
            {
                _logger.LogWarning(
                    "Could not retrieve the Microsoft user.");

                return null;
            }

            var email = !string.IsNullOrWhiteSpace(user.Mail)
                ? user.Mail
                : user.UserPrincipalName;

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning(
                    "Microsoft user has no usable email or username.");

                return null;
            }

            var name = !string.IsNullOrWhiteSpace(user.DisplayName)
                ? user.DisplayName
                : email;

            return new ExternalUserInfo(
                user.Id,
                email,
                name);
        } 
        private sealed class MicrosoftTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }

            [JsonPropertyName("token_type")]
            public string? TokenType { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("error")]
            public string? Error { get; set; }
        }
        private sealed class MicrosoftUser
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("displayName")]
            public string? DisplayName { get; set; }

            [JsonPropertyName("mail")]
            public string? Mail { get; set; }

            [JsonPropertyName("userPrincipalName")]
            public string? UserPrincipalName { get; set; }
        }

    }
}
