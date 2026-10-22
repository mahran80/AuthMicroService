namespace Authentication.Application.DTOs
{
    public class SocialLoginRequest
    {
        public string Provider { get; set; } = string.Empty;
        public string Credential { get; set; } = string.Empty;
        //OAuth
        public string? CodeVerifier { get; set; }
    }
}
