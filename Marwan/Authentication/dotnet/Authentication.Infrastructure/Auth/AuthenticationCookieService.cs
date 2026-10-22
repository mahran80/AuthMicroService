using Authentication.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace Authentication.Infrastructure.Auth
{
    public class AuthenticationCookieService(IHttpContextAccessor _httpContextAccessor)
        : IAuthenticationCookieService
    {
        public void SetAccessToken(string token, DateTimeOffset expiresAt)
        {
            var response = GetResponse();

            response.Cookies.Append(
                AuthenticationCookieNames.AccessToken,
                token,
                CreateCookieOptions(expiresAt));
        }

        public void SetRefreshToken(string token, DateTimeOffset expiresAt)
        {
            var response = GetResponse();

            response.Cookies.Append(
                AuthenticationCookieNames.RefreshToken,
                token,
                CreateCookieOptions(expiresAt));
        }

        public void ClearAuthenticationCookies()
        {
            var response = GetResponse();

            response.Cookies.Delete(
                AuthenticationCookieNames.AccessToken);

            response.Cookies.Delete(
                AuthenticationCookieNames.RefreshToken);
        }

        //Helpers
        private HttpResponse GetResponse()
        {
            return _httpContextAccessor.HttpContext?.Response
                ?? throw new InvalidOperationException(
                    "No active HTTP response is available.");
        }

        private static CookieOptions CreateCookieOptions(DateTimeOffset expiresAt)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = expiresAt
            };
        }
    }
}