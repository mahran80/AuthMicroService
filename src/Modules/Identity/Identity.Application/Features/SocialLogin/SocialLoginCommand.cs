using Identity.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.SocialLogin;

public sealed record SocialLoginCommand(
    string Provider,
    string Credential) : ICommand<AuthTokenResponse>;
