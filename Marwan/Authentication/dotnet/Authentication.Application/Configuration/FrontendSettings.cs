namespace Authentication.Application.Configuration
{
    public class FrontendSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string EmailVerificationPath { get; set; } = "/verify-email";
        public string PasswordResetPath { get; set; } = "/reset-password";
    }
}
