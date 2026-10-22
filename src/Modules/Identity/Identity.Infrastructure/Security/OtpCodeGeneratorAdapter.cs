using System.Security.Cryptography;
using System.Text;
using Identity.Application.Abstractions.Ports;

namespace Identity.Infrastructure.Security;

public sealed class OtpCodeGeneratorAdapter : IOtpCodeGeneratorPort
{
    public string GenerateSixDigitCode()
    {
        int code = RandomNumberGenerator.GetInt32(100000, 1000000);
        return code.ToString("D6");
    }

    public string GenerateSecureUrlToken()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(tokenBytes).ToLowerInvariant();
    }

    public string ComputeSha256Hash(string rawValue)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(rawValue);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
