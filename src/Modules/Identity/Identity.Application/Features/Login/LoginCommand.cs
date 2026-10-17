using Identity.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<AuthTokenResponse>;
