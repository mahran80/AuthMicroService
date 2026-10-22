namespace Identity.Application.DTOs;

public sealed record ExternalUserInfo(
    string Provider,
    string ProviderUserId,
    string Email,
    string FirstName,
    string LastName,
    bool EmailVerified);
