namespace Identity.Infrastructure.Social;

public sealed class SocialAuthOptions
{
    public const string SectionName = "Authentication";

    public GoogleOptions Google { get; set; } = new();
    public FacebookOptions Facebook { get; set; } = new();
    public GitHubOptions GitHub { get; set; } = new();
    public LinkedInOptions LinkedIn { get; set; } = new();
    public MicrosoftOptions Microsoft { get; set; } = new();
    public XOptions X { get; set; } = new();
}

public sealed class GoogleOptions
{
    public string ClientId { get; set; } = string.Empty;
}

public sealed class FacebookOptions
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}

public sealed class GitHubOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}

public sealed class LinkedInOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}

public sealed class MicrosoftOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string TenantId { get; set; } = "common";
}

public sealed class XOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
