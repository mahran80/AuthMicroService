namespace Auth.Application.Ports.Outbound;

public interface IOtpService
{
    string GenerateOtp();

    DateTime GetOtpExpiration();
}