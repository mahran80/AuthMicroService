namespace Notifications.WhatsApp.Infrastructure.Configuration
{
    /// <summary>
    /// Bound from the "UltraMsg" section of appsettings / environment / secrets.
    /// </summary>
    public class UltraMsgOptions
    {
        public const string SectionName = "UltraMsg";

        /// <summary>
        /// Your UltraMsg instance id, e.g. "instance123456".
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// UltraMsg API token for the instance.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Base API host. Defaults to https://api.ultramsg.com and rarely needs changing.
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.ultramsg.com";
    }
}
