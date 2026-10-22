using Auth.Application.DTOs.RefreshToken.Request;
using Auth.Application.DTOs.RefreshToken.Response;
using Auth.Application.Ports.Outbound;
using Auth.Domain.Entities;

namespace Auth.Application.UseCases.RefreshToken.Commands;

public sealed class RefreshTokenCommand
{
    private readonly IAuthenticationRepository _repository;

    private readonly IJwtService _jwtService;

    public RefreshTokenCommand(
        IAuthenticationRepository repository,
        IJwtService jwtService)
    {
        _repository = repository;
        _jwtService = jwtService;
    }

    public async Task<RefreshTokenResponse> ExecuteAsync(
        RefreshTokenRequest request)
    {
        var refreshToken =
            await _repository.GetRefreshTokenAsync(request.RefreshToken);

        if (refreshToken is null)
            throw new Exception("Invalid Refresh Token");

        if (refreshToken.ExpireAt < DateTime.UtcNow)
            throw new Exception("Refresh Token Expired");

        var accessToken =
            _jwtService.GenerateAccessToken(refreshToken.UserId);

        var newRefresh =
            _jwtService.GenerateRefreshToken();

        refreshToken.Token = newRefresh;

refreshToken.ExpireAt =
    _jwtService.GetRefreshTokenExpiration();

        await _repository.UpdateRefreshTokenAsync(refreshToken);

        return new RefreshTokenResponse
        {
 
    AccessToken = accessToken,
RefreshToken = newRefresh
};
        }
    }
