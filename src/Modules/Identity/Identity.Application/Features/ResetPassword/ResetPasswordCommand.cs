using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.ResetPassword;

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword) : ICommand;
