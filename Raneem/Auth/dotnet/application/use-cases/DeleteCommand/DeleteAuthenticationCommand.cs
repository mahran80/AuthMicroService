using Auth.Application.Ports.Outbound;
namespace Auth.Application.UseCases.Commands;
public sealed class DeleteAuthenticationCommand
{
    private readonly IAuthenticationRepository _repository;

    public DeleteAuthenticationCommand(
        IAuthenticationRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Guid userId)
    {
        await _repository.DeleteAuthenticationAsync(userId);
    }
}