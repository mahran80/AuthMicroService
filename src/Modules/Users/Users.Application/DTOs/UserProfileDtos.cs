namespace Users.Application.DTOs;

public sealed record UserProfileResponse(
    Guid UserId,
    string FullName,
    string Email,
    string? CountryCode,
    string? PhoneNumber,
    bool IsDeleted,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
