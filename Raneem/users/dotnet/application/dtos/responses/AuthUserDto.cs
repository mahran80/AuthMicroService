namespace Users.Application.DTOs.Responses;

public sealed class AuthUserDto
{
    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string CountryCode { get; set; } = "";

    public string PhoneNumber { get; set; } = "";
}