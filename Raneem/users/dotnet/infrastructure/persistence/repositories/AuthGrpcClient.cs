using Microsoft.EntityFrameworkCore;
using Users.Application.Ports.Outbound;
using Auth.Grpc;
using  Users.Application.DTOs.Responses;

using Users.Domain.Entities;
using Users.Infrastructure.Persistence.Context;

namespace Users.Infrastructure.Persistence.Repositories;
public sealed class AuthGrpcClient : IAuthGrpcClient
{
    private readonly AuthService.AuthServiceClient _client;


    public AuthGrpcClient(AuthService.AuthServiceClient client)
    {
        _client = client;
    }
//==================================================================
public async Task DeleteAuthenticationAsync(Guid userId)
{
    await _client.DeleteAuthenticationAsync(
        new DeleteAuthenticationRequest
        {
            UserId = userId.ToString()
        });
}
//=========================================================
    public async Task<AuthUserDto?> GetUserAuthInfoAsync(Guid userId)
    {
        var response =
            await _client.GetUserAuthInfoAsync(
                new GetUserAuthInfoRequest
                {
                    UserId = userId.ToString()
                });


        if(!response.Success)
            return null;


        return new AuthUserDto
        {
            Name = response.Name,
            Email = response.Email,
            CountryCode = response.CountryCode,
            PhoneNumber = response.PhoneNumber
        };
    }
}