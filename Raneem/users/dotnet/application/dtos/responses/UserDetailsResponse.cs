namespace Users.Application.DTOs.Responses;

public sealed class UserDetailsResponse
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
}