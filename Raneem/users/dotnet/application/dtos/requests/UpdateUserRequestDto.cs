namespace Users.Application.DTOs.Requests;

public sealed class UpdateUserRequestDto
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
}