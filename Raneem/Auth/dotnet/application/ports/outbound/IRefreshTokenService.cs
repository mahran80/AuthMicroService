namespace Auth.Application.Ports.Outbound;

public interface IRefreshTokenService
{
    string Generate();
}