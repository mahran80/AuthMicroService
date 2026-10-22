using Grpc.Core;
using IAM.Grpc;
using Users.Application.Ports.Outbound;
using Users.Application.DTOs.Requests;
using Users.Application.UseCases.Commands;
using Users.Application.UseCases.Queries;

namespace IAM.Services;

public sealed class UserGrpcService : UserService.UserServiceBase
{
    private readonly CreateUserCommand _createUserCommand;
    private readonly UpdateUserCommand _updateUserCommand;
    private readonly DeleteUserCommand _deleteUserCommand;
    private readonly GetUserByIdQuery _getUserByIdQuery;
    private readonly GetUserByPhoneQuery _getUserByPhoneQuery;

private readonly IUserRepository _userRepository;
private readonly IAuthGrpcClient _authGrpcClient;

    public UserGrpcService(
        CreateUserCommand createUserCommand,
        UpdateUserCommand updateUserCommand,
        DeleteUserCommand deleteUserCommand,
        GetUserByPhoneQuery getUserByPhoneQuery,
        GetUserByIdQuery getUserByIdQuery,
     IUserRepository userRepository,
    IAuthGrpcClient authGrpcClient)
{
    _userRepository = userRepository;
    _authGrpcClient = authGrpcClient;
        _createUserCommand = createUserCommand;
        _updateUserCommand = updateUserCommand;
        _deleteUserCommand = deleteUserCommand;
        _getUserByPhoneQuery = getUserByPhoneQuery;
        _getUserByIdQuery = getUserByIdQuery;
    }
    //==========================================================
  public override async Task<CreateUserResponse> CreateUserD(
    CreateUserDRequest request,
    ServerCallContext context)
{
    var result = await _createUserCommand.ExecuteAsync(
        new CreateUserRequestDto
        {
            Name = request.Name,
            Email = request.Email,
            CountryCode = request.CountryCode,
            PhoneNumber = request.PhoneNumber
        });

    return new CreateUserResponse
    {
        UserId = result.UserId.ToString()
    };
}
    //===========================================================
    public override async Task<GetUserByPhoneResponse> GetUserByPhone(
        GetUserByPhoneRequest request,
        ServerCallContext context)
    {
        var userId = await _getUserByPhoneQuery.ExecuteAsync(
            request.PhoneNumber);

        if (userId is null)
        {
            return new GetUserByPhoneResponse
            {
                Success = false
            };
        }

        return new GetUserByPhoneResponse
        {
            Success = true,
            UserId = userId.Value.ToString()
        };
    }
    public override async Task<CreateUserResponse> CreateUser(
        CreateUserRequest request,
        ServerCallContext context)
    {
        var result = await _createUserCommand.ExecuteAsync(
            new CreateUserRequestDto
            {
                Email = request.Email,
                CountryCode = request.CountryCode,
                PhoneNumber = request.PhoneNumber
            });

        return new CreateUserResponse
        {
            UserId = result.UserId.ToString()
        };
    }
    public override async Task<DeleteUserResponse> DeleteUser(
    DeleteUserRequest request,
    ServerCallContext context)
    {
        await _deleteUserCommand.ExecuteAsync(
            Guid.Parse(request.UserId));

        return new DeleteUserResponse
        {
            Success = true
        };
    }

    public override async Task<UpdateUserResponse> UpdateUser(
        UpdateUserRequest request,
        ServerCallContext context)
    {
        await _updateUserCommand.ExecuteAsync(
            new UpdateUserRequestDto
            {
                UserId = Guid.Parse(request.UserId),
                Email = request.Email,
                CountryCode = request.CountryCode,
                PhoneNumber = request.PhoneNumber
            });

        return new UpdateUserResponse
        {
            Success = true
        };
    }
    public override async Task<GetUserByIdResponse> GetUserById(
    GetUserByIdRequest request,
    ServerCallContext context)
    { 
        // Console.WriteLine("===== Users GetUserById =====");
        var result = await _getUserByIdQuery.ExecuteAsync(
            Guid.Parse(request.UserId));

        if (result == null)
        {
            return new GetUserByIdResponse
            {
                Success = false
            };
        }

        return new GetUserByIdResponse
        {
            Success = true,
            UserId = result.UserId.ToString(),
            Name = result.Name,
            CountryCode = result.CountryCode,
            PhoneNumber = result.PhoneNumber ?? "",
            Email = result.Email ?? ""
        };
    }
}