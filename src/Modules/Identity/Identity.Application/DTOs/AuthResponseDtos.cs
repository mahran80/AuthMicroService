namespace Identity.Application.DTOs;

public sealed record RegisterUserResponse(
    Guid UserId,
    string Email,
    string AssignedDefaultRole,
    DateTime CreatedAtUtc);

public sealed record UserContextResponse(
    Guid Id,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc,
    UserContextResponse User);

public sealed record TokenPairResult(
    string AccessToken,
    string RefreshToken,
    string JwtId,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);

public sealed record RevokeTokenResponse(
    string Message);
