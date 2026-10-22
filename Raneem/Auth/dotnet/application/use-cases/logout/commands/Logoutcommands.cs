using Auth.Application.Ports.Outbound;

namespace Auth.Application.UseCases.Logout.Commands;

public sealed class LogoutCommand
{
    private readonly IAuthenticationRepository _authenticationRepository;

    public LogoutCommand(
        IAuthenticationRepository authenticationRepository)
    {
        _authenticationRepository = authenticationRepository;
    }

    public async Task ExecuteAsync(string refreshToken)
    {
        Console.WriteLine("========== LOGOUT ==========");
        Console.WriteLine($"Refresh Token: {refreshToken}");

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            Console.WriteLine("Refresh token is empty.");
            throw new Exception("Refresh token is missing.");
        }

        var token =
            await _authenticationRepository.GetRefreshTokenAsync(refreshToken);

        if (token is null)
        {
            Console.WriteLine("Refresh token NOT FOUND in database.");
            throw new Exception("Invalid refresh token.");
        }

        Console.WriteLine("Refresh token FOUND.");
        Console.WriteLine($"UserId     : {token.UserId}");
        Console.WriteLine($"IsRevoked  : {token.IsRevoked}");
        Console.WriteLine($"ExpireAt   : {token.ExpireAt}");

        if (token.IsRevoked)
        {
            Console.WriteLine("Refresh token already revoked.");
            throw new Exception("Refresh token already revoked.");
        }

        if (token.ExpireAt <= DateTime.UtcNow)
        {
            Console.WriteLine("Refresh token expired.");
            throw new Exception("Refresh token expired.");
        }

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        Console.WriteLine("Updating database...");

        await _authenticationRepository.UpdateRefreshTokenAsync(token);

        Console.WriteLine("Database updated successfully.");
        Console.WriteLine("========== END LOGOUT ==========");
    }
}