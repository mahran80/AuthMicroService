namespace Auth.Application.DTOs.Login.Request;

public sealed class LoginRequest
{
    public string ProjectType { get; set; } = null!;

    public string CountryCode { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

}