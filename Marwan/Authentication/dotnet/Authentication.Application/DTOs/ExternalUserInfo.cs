namespace Authentication.Application.DTOs
{
    public sealed record ExternalUserInfo(
        string ProviderUserId,
        string Email,
        string Name
    );
}
