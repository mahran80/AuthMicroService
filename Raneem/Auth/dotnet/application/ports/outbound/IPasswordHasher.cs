namespace Auth.Application.Ports.Outbound;

public interface IPasswordHasher
{
    bool Verify(string password, string hash);

    string Hash(string password);
}