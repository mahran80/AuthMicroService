using System;
using System.Threading.Tasks;
using Authentication.Infrastructure.Tests.TestDoubles;
using Xunit;

namespace Authentication.Infrastructure.Tests.SocialTests
{
    public class GoogleAuthServiceTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ValidateAsync_EmptyCredential_ReturnsNull(
            string credential)
        {
            // Arrange
            var service =GoogleAuthServiceTestDoubles.CreateService();

            // Act
            var result = await service.ValidateAsync(credential);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateAsync_MissingClientId_ThrowsException()
        {
            // Arrange
            var service =
                GoogleAuthServiceTestDoubles.CreateService(
                    includeClientId: false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ValidateAsync("fake-token"));
        }
    }
}