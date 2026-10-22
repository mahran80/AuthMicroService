using Identity.Application.DTOs;
using SharedKernel.Application;

namespace Identity.Application.Abstractions.Ports;

public interface ISocialAuthProviderPort
{
    string ProviderName { get; }
    Task<Result<ExternalUserInfo>> ValidateAsync(string credential, CancellationToken ct = default);
}
