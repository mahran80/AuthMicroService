using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Authentication.Infrastructure.Social
{
    public class XAuthService : IXService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IXService> _logger;
        public XAuthService(HttpClient httpClient, ILogger<XAuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<ExternalUserInfo?> ValidateAsync(string credential)
        {
            if (string.IsNullOrEmpty(credential))
            {
                _logger.LogError("credential is essential");
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.x.com/2/users/me?user.fields=id,name,username");
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(" validation failed with status {StatusCode},response.StatusCode");
                return null;
            }
            var result = await response.Content
                   .ReadFromJsonAsync<XUserResponse>();

            var user = result?.Data;

            if (user is null || string.IsNullOrWhiteSpace(user.Id))
            {
                _logger.LogWarning(
                    "X returned an invalid user profile.");

                return null;
            }
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning(
                    "X user {UserId} has no available email.",
                    user.Id);

                return null;
            }

            return new ExternalUserInfo(
                user.Id,
                user.Email,
                user.Name ?? user.Username ?? string.Empty);
        }
        private sealed class XUserResponse
        {
            [JsonPropertyName("data")]
            public XUser? Data { get; set; }
        }

        private sealed class XUser
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("username")]
            public string? Username { get; set; }

            [JsonPropertyName("email")]
            public string? Email { get; set; }
        }
    }
}
