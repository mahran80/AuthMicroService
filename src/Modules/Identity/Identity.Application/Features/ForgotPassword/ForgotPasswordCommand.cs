using Identity.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.ForgotPassword;

public sealed record ForgotPasswordCommand(
    string Email) : ICommand<PasswordResetDispatchedResponse>;
