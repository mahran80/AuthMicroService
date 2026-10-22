namespace Identity.Presentation.Contracts;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? CountryCode = null,
    string? PhoneNumber = null);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record RefreshTokenRequest(
    string RefreshToken);

public sealed record RevokeTokenRequest(
    string RefreshToken);

public sealed record LoginMobileRequest(
    string CountryCode,
    string PhoneNumber);

public sealed record VerifyOtpRequest(
    string? Email,
    string? CountryCode,
    string? PhoneNumber,
    string Code,
    string Purpose,
    string? DeviceName = null,
    string? DeviceType = null);

public sealed record ResendOtpRequest(
    string? Email,
    string? CountryCode,
    string? PhoneNumber,
    string Purpose);

public sealed record ForgotPasswordRequest(
    string Email);

public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword);

public sealed record SocialLoginRequest(
    string Provider,
    string Credential);
