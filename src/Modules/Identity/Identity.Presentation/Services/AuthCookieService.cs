using Microsoft.AspNetCore.Http;

namespace Identity.Presentation.Services;

public sealed class AuthCookieService : IAuthCookieService
{
    public const string AccessTokenCookieName = "accessToken";
    public const string RefreshTokenCookieName = "refreshToken";

    public void SetTokenCookies(
        HttpResponse response,
        string accessToken,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc)
    {
        response.Cookies.Append(
            AccessTokenCookieName,
            accessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = accessTokenExpiresAtUtc,
                Path = "/"
            });

        response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshTokenExpiresAtUtc,
                Path = "/"
            });
    }

    public void ClearTokenCookies(HttpResponse response)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };

        response.Cookies.Delete(AccessTokenCookieName, options);
        response.Cookies.Delete(RefreshTokenCookieName, options);
    }
}
