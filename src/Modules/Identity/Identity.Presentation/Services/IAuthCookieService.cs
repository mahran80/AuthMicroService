using Microsoft.AspNetCore.Http;

namespace Identity.Presentation.Services;

public interface IAuthCookieService
{
    void SetTokenCookies(
        HttpResponse response,
        string accessToken,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc);

    void ClearTokenCookies(HttpResponse response);
}
