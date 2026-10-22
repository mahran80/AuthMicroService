using Authentication.Application.DTOs;
using System.Threading.Tasks;

namespace Authentication.Application.Interfaces
{
    public interface IFacebookAuthService
    {
        Task<ExternalUserInfo?> ValidateAsync(string credential);
    }
}
