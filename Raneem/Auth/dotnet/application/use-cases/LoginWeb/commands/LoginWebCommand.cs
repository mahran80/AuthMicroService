using Auth.Application.DTOs.LoginWeb.Request;
using Auth.Application.DTOs.LoginWeb.Response;
using Auth.Application.Ports.Outbound;
using Auth.Application.Ports.Outbound.web;
using Auth.Domain.Entities;

namespace Auth.Application.UseCases.LoginWeb.Commands;

public sealed class LoginWebCommand
{
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtServiceWeb _jwtService;

    public LoginWebCommand(
        IAuthenticationRepository authenticationRepository,
        IPasswordHasher passwordHasher,
        IJwtServiceWeb jwtService)
    {
        _authenticationRepository = authenticationRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginWebResponse> ExecuteAsync(
        LoginWebRequest request)
    {
        // Get User
        var user = await _authenticationRepository.GetByEmailAsync(request.Email);

        if (user is null)
        {
            return new LoginWebResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        // Verify Password
        var isPasswordValid = _passwordHasher.Verify(
            request.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            return new LoginWebResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        // Generate Tokens
        var accessToken =
            _jwtService.GenerateAccessToken(user.UserId);

        var refreshToken =
            _jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new Auth.Domain.Entities.RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = user.UserId,
            Token = refreshToken,
            ExpireAt =
    _jwtService.GetRefreshTokenExpiration(
        request.RememberMe),
            IsRevoked = false,
            RevokedAt = null
        };

        await _authenticationRepository.SaveRefreshTokenAsync(
            refreshTokenEntity);

        return new LoginWebResponse
        {
            Success = true,
            Message = "Login successful.",

            AccessToken = accessToken,
            AccessTokenExpiresAt =
                _jwtService.GetAccessTokenExpiration(),

            RefreshToken = refreshToken,
            RefreshTokenExpiresAt =
                refreshTokenEntity.ExpireAt,
                 RememberMe = request.RememberMe
        };
    }
}