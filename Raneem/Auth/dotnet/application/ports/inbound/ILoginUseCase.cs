using Auth.Application.DTOs.Login.Request;
using Auth.Application.DTOs.Login.Response;

namespace Auth.Application.Ports.Inbound;

public interface ILoginUseCase
{
    Task<LoginResponse> ExecuteAsync(LoginRequest request);
}