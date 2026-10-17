namespace Identity.Application.Abstractions.Ports;

public interface IPasswordHasherPort
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
