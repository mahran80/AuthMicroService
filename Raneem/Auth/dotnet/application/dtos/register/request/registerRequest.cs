namespace Auth.Application.DTOs.Register.Request;

public sealed class RegisterRequest
{
    public string ProjectType { get; set; } = null!;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? CountryCode { get; set; }

    public string? PhoneNumber { get; set; }
}