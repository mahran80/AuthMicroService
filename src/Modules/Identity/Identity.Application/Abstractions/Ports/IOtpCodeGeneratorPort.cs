namespace Identity.Application.Abstractions.Ports;

public interface IOtpCodeGeneratorPort
{
    string GenerateSixDigitCode();
    string GenerateSecureUrlToken();
    string ComputeSha256Hash(string rawValue);
}
