using Users.Application.Ports.Outbound;
using Users.Domain.Entities;
using Users.Application.DTOs.Responses;

namespace Users.Application.UseCases.Queries;

public sealed class GetUserByIdQuery
{
 private readonly IUserRepository _userRepository;
    private readonly IAuthGrpcClient _authGrpcClient;

    public GetUserByIdQuery(
        IUserRepository userRepository,
        IAuthGrpcClient authGrpcClient)
    {
        _userRepository = userRepository;
        _authGrpcClient = authGrpcClient;
    }
 public async Task<UserDetailsResponse?> ExecuteAsync(Guid userId)
{
    // Console.WriteLine($"Searching Email for {userId}");

    var user = await _userRepository.GetByIdAsync(userId);

    if (user == null)
        return null;

var authInfo =
    await _authGrpcClient.GetUserAuthInfoAsync(userId);

return new UserDetailsResponse
{
    UserId = user.UserId,

    Name = authInfo?.Name ?? "",

    Email = authInfo?.Email ?? "",

    CountryCode = authInfo?.CountryCode ?? "",

    PhoneNumber = authInfo?.PhoneNumber ?? ""
};
}
}