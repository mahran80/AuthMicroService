using Users.Application.Ports.Outbound;

namespace Users.Application.UseCases.Commands;

public sealed class DeleteUserCommand
{
   private readonly IUserRepository _repository;
    private readonly IAuthGrpcClient _authGrpcClient;

    public DeleteUserCommand(
        IUserRepository repository,
        IAuthGrpcClient authGrpcClient)
    {
        _repository = repository;
        _authGrpcClient = authGrpcClient;
    }

     public async Task ExecuteAsync(Guid userId)
    {
        await _repository.DeleteAsync(userId);

        await _authGrpcClient.DeleteAuthenticationAsync(userId);
    }
}