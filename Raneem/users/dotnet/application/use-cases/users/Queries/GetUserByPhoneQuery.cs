using Users.Application.Ports.Outbound;

namespace Users.Application.UseCases.Queries;

public sealed class GetUserByPhoneQuery
{
    private readonly IUserRepository _repository;

    public GetUserByPhoneQuery(
        IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid?> ExecuteAsync(string phoneNumber)
    {
        var user = await _repository.GetByPhoneAsync(phoneNumber);

        return user?.UserId;
    }
}