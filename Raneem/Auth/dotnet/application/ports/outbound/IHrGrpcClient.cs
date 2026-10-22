namespace Auth.Application.Ports.Outbound;
public interface IHrGrpcClient
{
    Task CreatePassengerAsync(Guid userId);
}