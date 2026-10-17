namespace Identity.Domain.Entities;

using Identity.Domain.Enums;
using SharedKernel.Domain;

public sealed class User : Entity
{
    private readonly List<RefreshToken> _refreshTokens = new();

    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    private User(
        Guid id,
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        UserStatus status,
        DateTime createdAtUtc)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        Status = status;
        CreatedAtUtc = createdAtUtc;
    }

    public static User Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        return new User(
            Guid.NewGuid(),
            email.ToLowerInvariant().Trim(),
            passwordHash,
            firstName.Trim(),
            lastName.Trim(),
            UserStatus.Active,
            DateTime.UtcNow);
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = UserStatus.Deactivated;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        _refreshTokens.Add(refreshToken);
    }
}
