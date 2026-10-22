namespace Auth.Application.DTOs;

public sealed class AuthUserInfoDto
{
    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string CountryCode { get; set; } = "";

    public string PhoneNumber { get; set; } = "";
}