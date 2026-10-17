using Identity.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.Revoke;

public sealed record RevokeTokenCommand(
    string RefreshToken) : ICommand<RevokeTokenResponse>;
