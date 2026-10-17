using Identity.Application.Abstractions.Ports;

namespace Identity.Infrastructure.Security;

public sealed class BcryptPasswordHasherAdapter : IPasswordHasherPort
{
    private const int WorkFactor = 11;

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
    }
}
