namespace Identity.Infrastructure.Notifications;

public sealed class UltraMsgOptions
{
    public const string SectionName = "UltraMsg";

    public string InstanceId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.ultramsg.com";
}
