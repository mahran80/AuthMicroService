using System;

namespace Authentication.Application.Interfaces
{
    public interface IAuthenticationCookieService
    {
        void SetAccessToken(string token, DateTimeOffset expiresAt);
        void SetRefreshToken(string token, DateTimeOffset expiresAt);
        void ClearAuthenticationCookies();  
    }
}
