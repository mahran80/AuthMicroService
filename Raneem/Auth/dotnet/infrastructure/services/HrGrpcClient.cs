using Auth.Application.Ports.Outbound;
using Grpc.Net.Client;
using HrGrpc;

namespace Auth.Infrastructure.Services;

public sealed class HrGrpcClient : IHrGrpcClient
{
    private readonly HrService.HrServiceClient _client;

    public HrGrpcClient(HrService.HrServiceClient client)
    {
        _client = client;
    }

    public async Task CreatePassengerAsync(Guid userId)
    {
        await _client.CreatePassengerAsync(
            new CreatePassengerRequest
            {
                UserId = userId.ToString()
            });
    }
}