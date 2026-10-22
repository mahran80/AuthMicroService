using Authentication.Domain.Entities;
using Authentication.Infrastructure.Auth;
using Authentication.Infrastructure.Tests.TestDoubles;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.Infrastructure.Tests.AuthTests
{
    public class AccessTokenServiceTest
    {
        private static readonly JwtSettings Settings =
            JwtSettingsTestDoubles.CreateSettings().Value;

        private static AccessTokenService CreateService(
            IList<string>? roles = null)
        {
            var userManager = MockUserManager.Create(roles);

            return new AccessTokenService(
                Options.Create(Settings),
                userManager.Object);
        }

        private static ApplicationUser CreateUser() =>
            JwtSettingsTestDoubles.CreateUser();

        [Fact]
        public async Task GenerateAsync_ValidUser_ReturnsValidJwt()
        {
            // Arrange
            var service = CreateService();
            var user = CreateUser();

            // Act
            var result = await service.GenerateAsync(user);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result.Token));

            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(
                result.Token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Settings.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = Settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = Settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out var validatedToken);

            Assert.IsType<JwtSecurityToken>(validatedToken);
            Assert.NotNull(principal);
        }

        [Fact]
        public async Task GenerateAsync_ValidUser_ContainsExpectedClaims()
        {
            // Arrange
            var roles = JwtSettingsTestDoubles.CreateRoles();
            var service = CreateService(roles);
            var user = CreateUser();

            // Act
            var result = await service.GenerateAsync(user);
            var jwt = new JwtSecurityTokenHandler()
                .ReadJwtToken(result.Token);

            // Assert
            Assert.Contains(jwt.Claims, c =>
                c.Type == JwtRegisteredClaimNames.Sub &&
                c.Value == user.Id.ToString());

            Assert.Contains(jwt.Claims, c =>
                c.Type == JwtRegisteredClaimNames.Email &&
                c.Value == user.Email);

            Assert.Contains(jwt.Claims, c =>
                c.Type == ClaimTypes.NameIdentifier &&
                c.Value == user.Id.ToString());

            Assert.Contains(jwt.Claims, c =>
                c.Type == ClaimTypes.Name &&
                c.Value == user.UserName);

            foreach (var role in roles)
            {
                Assert.Contains(jwt.Claims, c =>
                    c.Type == ClaimTypes.Role &&
                    c.Value == role);
            }
        }

        [Fact]
        public async Task GenerateAsync_UserWithoutRoles_DoesNotAddRoleClaims()
        {
            // Arrange
            var service = CreateService(new List<string>());
            var user = CreateUser();

            // Act
            var result = await service.GenerateAsync(user);
            var jwt = new JwtSecurityTokenHandler()
                .ReadJwtToken(result.Token);

            // Assert
            Assert.DoesNotContain(
                jwt.Claims,
                c => c.Type == ClaimTypes.Role);
        }

        [Fact]
        public async Task GenerateAsync_ExpirationMatchesConfiguredDuration()
        {
            // Arrange
            var service = CreateService();
            var user = CreateUser();
            var beforeGeneration = DateTimeOffset.UtcNow;

            // Act
            var result = await service.GenerateAsync(user);
            var afterGeneration = DateTimeOffset.UtcNow;

            var jwt = new JwtSecurityTokenHandler()
                .ReadJwtToken(result.Token);

            var jwtExpiration = new DateTimeOffset(
                DateTime.SpecifyKind(
                    jwt.ValidTo,
                    DateTimeKind.Utc));

            // Assert
            Assert.Equal(
                result.ExpiresAt.ToUnixTimeSeconds(),
                jwtExpiration.ToUnixTimeSeconds());

            Assert.InRange(
                result.ExpiresAt,
                beforeGeneration.AddMinutes(
                    Settings.AccessTokenExpirationMinutes),
                afterGeneration.AddMinutes(
                    Settings.AccessTokenExpirationMinutes));
        }

        [Fact]
        public async Task GenerateAsync_UsesConfiguredIssuerAndAudience()
        {
            // Arrange
            var service = CreateService();
            var user = CreateUser();

            // Act
            var result = await service.GenerateAsync(user);
            var jwt = new JwtSecurityTokenHandler()
                .ReadJwtToken(result.Token);

            // Assert
            Assert.Equal(Settings.Issuer, jwt.Issuer);
            Assert.Contains(Settings.Audience, jwt.Audiences);
        }
    }
}