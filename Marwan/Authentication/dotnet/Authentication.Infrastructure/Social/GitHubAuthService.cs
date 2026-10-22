using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Authentication.Infrastructure.Social
{
    public sealed class GitHubAuthService : IGitHubAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GitHubAuthService> _logger;
        private readonly IConfiguration _configuration;
        public GitHubAuthService(HttpClient httpClient, ILogger<GitHubAuthService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<string?> ExchangeCodeForTokenAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogError("code is empty");
                return null;
            }
            var clientId = _configuration["GitHub:ClientId"];
            var clientSecret = _configuration["Github:ClientSecret"];
            var redirectUri = _configuration["GitHub:RedirectUri"];
            if (string.IsNullOrEmpty(clientId)||string.IsNullOrEmpty(clientSecret)||string.IsNullOrEmpty(redirectUri))
            {
                _logger.LogError("GitHub OAuth configuration is incomplete.");
                return null;
            }
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
            {
                Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["code"] = code,
                    ["redirect_uri"] = redirectUri
                })
            };
            using var response=await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GitHub token endpoint returned {StatusCode}.",response.StatusCode);
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<GitHubTokenResponse>();
            if (!string.IsNullOrWhiteSpace(result?.Error))
            {
                _logger.LogWarning(
                    "GitHub token exchange failed: {Error}.",
                    result.Error);

                return null;
            }
            return result.AccessToken;
        }
        public async Task<GitHubUser?> GetUserAsync(string accessToken)
        {
            var request=new HttpRequestMessage(HttpMethod.Get,"https://api.github.com/user");
            request.Headers.Authorization=new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.UserAgent.ParseAdd("AuthenticationApp/1.0");

            var response=await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;        
            }

            var user = await response.Content.ReadFromJsonAsync<GitHubUser>();
            if (user is null)
                return null;

            return new GitHubUser
            {
                Id = user.Id,
                Login = user.Login,
                Name = user.Name,
                Email = user.Email
            };
        }
        public async Task<ExternalUserInfo?> ValidateAsync(string Credential)
        {
            var accesstoken=await ExchangeCodeForTokenAsync(Credential);
            if (string.IsNullOrEmpty(accesstoken)) 
            {
                _logger.LogError("accesstoken is essential");
                return null;
            }
            var user = await GetUserAsync(accesstoken);
            if (user is null) 
                return null;

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning("GitHub user has no public email.");
                return null;
            }

            return new ExternalUserInfo(
                user.Id.ToString(),
                user.Email,
                user.Name ?? user.Login ?? string.Empty);
        }
        private sealed class GitHubTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }

            [JsonPropertyName("error")]
            public string? Error { get; set; }
        }
        public sealed class GitHubUser
        {
            public long Id { get; set; }
            public string? Login { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
        }
    }
} 