using Auth.Application.Ports.Outbound;
using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.Services.Security;

public sealed class OtpService : IOtpService
{
    private readonly IConfiguration _configuration;

    public OtpService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateOtp()
    {
        var random = new Random();

        return Random.Shared.Next(10000, 99999).ToString();
    }

    public DateTime GetOtpExpiration()
    {
        var minutes = int.Parse(
            _configuration["Otp:ExpirationMinutes"]!);

        return DateTime.UtcNow.AddMinutes(minutes);
    }
}