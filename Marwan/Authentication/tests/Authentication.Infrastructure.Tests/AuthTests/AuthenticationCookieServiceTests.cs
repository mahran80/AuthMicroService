
using Authentication.Infrastructure.Auth;
using Authentication.Infrastructure.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using Xunit;

namespace Authentication.Infrastructure.Tests.AuthTests
{
    public class AuthenticationCookieServiceTests
    {
        [Fact]
        public void SetAccessToken_ShouldAppendAccessTokenCookie()
        {
            // Arrange
            var mock = new MockAuthenticationCookieService();

            const string token = "test-access-token";
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

            // Act
            mock.Service.SetAccessToken(token, expiresAt);

            // Assert
            mock.ResponseCookies.Verify(
                c => c.Append(
                    AuthenticationCookieNames.AccessToken,
                    token,
                    It.Is<CookieOptions>(o =>
                        o.HttpOnly &&
                        o.Secure &&
                        o.SameSite == SameSiteMode.Lax &&
                        o.Expires == expiresAt)),
                Times.Once);
        }

        [Fact]
        public void SetRefreshToken_ShouldAppendRefreshTokenCookie()
        {
            // Arrange
            var mock = new MockAuthenticationCookieService();

            const string token = "test-refresh-token";
            var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

            // Act
            mock.Service.SetRefreshToken(token, expiresAt);

            // Assert
            mock.ResponseCookies.Verify(
                c => c.Append(
                    AuthenticationCookieNames.RefreshToken,
                    token,
                    It.Is<CookieOptions>(o =>
                        o.HttpOnly &&
                        o.Secure &&
                        o.SameSite == SameSiteMode.Lax &&
                        o.Expires == expiresAt)),
                Times.Once);
        }
        [Fact]
        public void ClearAuthenticationCookies_ShouldDeleteBothCookies()
        {
            // Arrange
            var mock = new MockAuthenticationCookieService();

            // Act
            mock.Service.ClearAuthenticationCookies();

            // Assert
            mock.ResponseCookies.Verify(
                c => c.Delete(
                    AuthenticationCookieNames.AccessToken),
                Times.Once);

            mock.ResponseCookies.Verify(
                c => c.Delete(
                    AuthenticationCookieNames.RefreshToken),
                Times.Once);
        }
        [Fact]
        public void SetAccessToken_WhenHttpContextIsMissing_ShouldThrow()
        {
            // Arrange
            var accessor = new Mock<IHttpContextAccessor>();

            accessor
                .SetupGet(a => a.HttpContext)
                .Returns((HttpContext?)null);

            var service = new AuthenticationCookieService(
                accessor.Object);

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => service.SetAccessToken(
                    "test-access-token",
                    DateTimeOffset.UtcNow.AddMinutes(30)));
        }
        [Fact]
        public void SetRefreshToken_WhenHttpContextIsMissing_ShouldThrow()
        {
            // Arrange
            var accessor = new Mock<IHttpContextAccessor>();

            accessor
                .SetupGet(a => a.HttpContext)
                .Returns((HttpContext?)null);

            var service = new AuthenticationCookieService(
                accessor.Object);

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => service.SetRefreshToken(
                    "test-refresh-token",
                    DateTimeOffset.UtcNow.AddDays(7)));
        }
        [Fact]
        public void ClearAuthenticationCookies_WhenHttpContextIsMissing_ShouldThrow()
        {
            // Arrange
            var accessor = new Mock<IHttpContextAccessor>();

            accessor
                .SetupGet(a => a.HttpContext)
                .Returns((HttpContext?)null);

            var service = new AuthenticationCookieService(
                accessor.Object);

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => service.ClearAuthenticationCookies());
        }
    }
}
