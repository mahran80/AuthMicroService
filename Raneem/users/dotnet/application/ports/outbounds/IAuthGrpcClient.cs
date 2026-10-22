using Users.Application.DTOs.Responses;
namespace Users.Application.Ports.Outbound;

public interface IAuthGrpcClient
{
    Task<AuthUserDto?> GetUserAuthInfoAsync(Guid userId);
    
    Task DeleteAuthenticationAsync(Guid userId);
}