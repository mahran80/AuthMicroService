using Grpc.Core;
using Auth.Grpc;
using Auth.Application.Ports.Outbound;
using Auth.Application.UseCases.Commands;

namespace Auth.Services;

public sealed class AuthGrpcService : AuthService.AuthServiceBase
{
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly DeleteAuthenticationCommand _deleteAuthenticationCommand;

public AuthGrpcService(
    IAuthenticationRepository authenticationRepository,
    DeleteAuthenticationCommand deleteAuthenticationCommand)
{
    _authenticationRepository = authenticationRepository;
    _deleteAuthenticationCommand = deleteAuthenticationCommand;
}

public override async Task<DeleteAuthenticationReply> DeleteAuthentication(
    DeleteAuthenticationRequest request,
    ServerCallContext context)
{
    await _deleteAuthenticationCommand.ExecuteAsync(
        Guid.Parse(request.UserId));

    return new DeleteAuthenticationReply
    {
        Success = true,
        Message = "Authentication deleted successfully."
    };
}
 public override async Task<GetUserAuthInfoResponse> GetUserAuthInfo(
    GetUserAuthInfoRequest request,
    ServerCallContext context)
{
    var userId = Guid.Parse(request.UserId);


    var authInfo =
        await _authenticationRepository.GetUserAuthInfoAsync(userId);


    if(authInfo == null)
    {
        return new GetUserAuthInfoResponse
        {
            Success = false
        };
    }

// Console.WriteLine($"Name: {authInfo.Name ?? "NULL"}");
// Console.WriteLine($"Email: {authInfo.Email ?? "NULL"}");
// Console.WriteLine($"CountryCode: {authInfo.CountryCode ?? "NULL"}");
// Console.WriteLine($"PhoneNumber: {authInfo.PhoneNumber ?? "NULL"}");
    return new GetUserAuthInfoResponse
    {
        Success = true,

    UserId = userId.ToString(),

    Name = authInfo.Name ?? string.Empty,

    Email = authInfo.Email ?? string.Empty,

    CountryCode = authInfo.CountryCode ?? string.Empty,

    PhoneNumber = authInfo.PhoneNumber ?? string.Empty
    };
}
}