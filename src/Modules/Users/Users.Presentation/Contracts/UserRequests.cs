namespace Users.Presentation.Contracts;

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string? CountryCode,
    string? PhoneNumber);

public sealed record UpdateUserRequest(
    string FullName,
    string Email,
    string? CountryCode,
    string? PhoneNumber);
