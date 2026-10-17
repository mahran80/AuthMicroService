using Identity.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.Refresh;

public sealed record RefreshTokenCommand(
    string RefreshToken) : ICommand<AuthTokenResponse>;
