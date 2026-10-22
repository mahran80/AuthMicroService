using Auth.Application.Ports.Outbound;

namespace Auth.Infrastructure.Services.Security;

public sealed class RefreshTokenService : IRefreshTokenService
{
    public string Generate()
    {
        return Guid.NewGuid().ToString("N");
    }
}