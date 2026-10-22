using Authentication.Infrastructure.Tests.TestDoubles;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.Infrastructure.Tests.SocialTests
{
    public class FacebookAuthServiceTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ValidateAsync_EmptyCredential_ReturnsNullWithoutRequest(
            string credential)
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                _ => throw new InvalidOperationException(
                    "No HTTP request should be sent."));

            var service =
                FacebookAuthServiceTestDoubles.CreateService(handler);

            // Act
            var result = await service.ValidateAsync(credential);

            // Assert
            Assert.Null(result);
            Assert.Equal(0, handler.RequestCount);
        }

        [Fact]
        public async Task ValidateAsync_ValidResponse_ReturnsUserInfo()
        {
            // Arrange
            var handler =
                FacebookAuthServiceTestDoubles.CreateHandler(
                    HttpStatusCode.OK,
                    FacebookAuthServiceTestDoubles.GetJson(
                        "ValidProfile"));

            var service =
                FacebookAuthServiceTestDoubles.CreateService(handler);

            // Act
            var result = await service.ValidateAsync(
                FacebookAuthServiceTestDoubles.GetValue(
                    "Credentials.Valid"));

            // Assert
            Assert.NotNull(result);
            Assert.Equal("facebook-123", result.ProviderUserId);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("Test User", result.Name);
            Assert.Equal(1, handler.RequestCount);
        }

        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task ValidateAsync_HttpError_ReturnsNull(
            HttpStatusCode statusCode)
        {
            // Arrange
            var handler =
                FacebookAuthServiceTestDoubles.CreateHandler(
                    statusCode,
                    FacebookAuthServiceTestDoubles.GetJson(
                        "ErrorResponse"));

            var service =
                FacebookAuthServiceTestDoubles.CreateService(handler);

            // Act
            var result = await service.ValidateAsync(
                FacebookAuthServiceTestDoubles.GetValue(
                    "Credentials.Invalid"));

            // Assert
            Assert.Null(result);
            Assert.Equal(1, handler.RequestCount);
        }

        [Theory]
        [InlineData("MissingId")]
        [InlineData("EmptyId")]
        [InlineData("NullProfile")]
        public async Task ValidateAsync_InvalidProfile_ReturnsNull(
            string profileName)
        {
            // Arrange
            var handler =
                FacebookAuthServiceTestDoubles.CreateHandler(
                    HttpStatusCode.OK,
                    FacebookAuthServiceTestDoubles.GetJson(
                        $"InvalidProfiles.{profileName}"));

            var service =
                FacebookAuthServiceTestDoubles.CreateService(handler);

            // Act
            var result = await service.ValidateAsync(
                FacebookAuthServiceTestDoubles.GetValue(
                    "Credentials.Valid"));

            // Assert
            Assert.Null(result);
            Assert.Equal(1, handler.RequestCount);
        }

        [Fact]
        public async Task ValidateAsync_MalformedJson_ReturnsNull()
        {
            // Arrange
            var handler =
                FacebookAuthServiceTestDoubles.CreateHandler(
                    HttpStatusCode.OK,
                    FacebookAuthServiceTestDoubles.GetJson(
                        "InvalidProfiles.MalformedJson"));

            var service =
                FacebookAuthServiceTestDoubles.CreateService(handler);

            // Act
            var result = await service.ValidateAsync(
                FacebookAuthServiceTestDoubles.GetValue(
                    "Credentials.Valid"));

            // Assert
            Assert.Null(result);
            Assert.Equal(1, handler.RequestCount);
        }

        [Fact]
        public async Task ValidateAsync_SpecialCredential_EncodesCredential()
        {
            // Arrange
            var credential = FacebookAuthServiceTestDoubles.GetValue(
                "Credentials.Special");

            var handler =
                FacebookAuthServiceTestDoubles.CreateHandler(
                    HttpStatusCode.OK,
                    FacebookAuthServiceTestDoubles.GetJson(
                        "ValidProfile"));

            var service =
                FacebookAuthServiceTestDoubles.CreateService(handler);

            // Act
            var result = await service.ValidateAsync(credential);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(handler.LastRequestUrl);

            Assert.Contains(
                Uri.EscapeDataString(credential),
                handler.LastRequestUrl!);

            Assert.Equal(1, handler.RequestCount);
        }
    }
}