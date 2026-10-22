using Authentication.Application.DTOs;
using System.Threading.Tasks;

namespace Authentication.Application.Interfaces
{
    public interface IAuthenticationService
    {
        //Register
        Task RegisterAsync(RegisterRequest request);
        Task VerifyEmailAsync(VerifyEmailRequest request);
        Task VerifyOtpAsync(VerifyOtpRequest request);
        Task ResendVerificationAsync(ResendVerificationRequest request);

        //Login
        Task LoginAsync(LoginRequest request);
        Task SocialLoginAsync(SocialLoginRequest request);

        //Forget/Reset Password
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
    }
}
