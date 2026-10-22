using Authentication.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Infrastructure.Security
{
    public class OtpHasher : IOtpHasher
    {
        private readonly PasswordHasher<object> _passwordHasher = new();

        public string Hash(string otp)
        {
            return _passwordHasher.HashPassword(new object(), otp);
        }

        public bool Verify(string otp, string hash)
        {
            var result = _passwordHasher.VerifyHashedPassword(new object(), hash, otp);

            return result == PasswordVerificationResult.Success ||
                   result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
