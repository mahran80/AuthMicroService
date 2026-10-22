namespace Identity.Application.DTOs;

public sealed record OtpDispatchedResponse(
    string Message,
    int ExpiresInSeconds);

public sealed record PasswordResetDispatchedResponse(
    string Message);
