namespace Users.Application.DTOs.Responses;

public sealed class UpdateUserResponseDto
{
    public Guid UserId { get; set; }

    public string? Email { get; set; } = string.Empty;

    public string? CountryCode { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; } = string.Empty;

    public string? Message { get; set; } = string.Empty;
}