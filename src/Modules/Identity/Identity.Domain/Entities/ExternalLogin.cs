using SharedKernel.Domain;

namespace Identity.Domain.Entities;

public sealed class ExternalLogin : Entity
{
    public Guid UserId { get; private set; }
    public string Provider { get; private set; }
    public string ProviderKey { get; private set; }
    public string Email { get; private set; }
    public DateTime LinkedAtUtc { get; private init; }

    private ExternalLogin()
    {
        Provider = string.Empty;
        ProviderKey = string.Empty;
        Email = string.Empty;
    }

    private ExternalLogin(
        Guid id,
        Guid userId,
        string provider,
        string providerKey,
        string email,
        DateTime linkedAtUtc)
        : base(id)
    {
        UserId = userId;
        Provider = provider;
        ProviderKey = providerKey;
        Email = email;
        LinkedAtUtc = linkedAtUtc;
    }

    public static ExternalLogin Create(
        Guid userId,
        string provider,
        string providerKey,
        string email)
    {
        return new ExternalLogin(
            Guid.NewGuid(),
            userId,
            provider,
            providerKey,
            email.Trim().ToLowerInvariant(),
            DateTime.UtcNow);
    }
}
