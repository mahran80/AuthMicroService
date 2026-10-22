using Authentication.Application.DTOs;
using System.Threading.Tasks;

namespace Authentication.Application.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<ExternalUserInfo?> ValidateAsync(string credential);
    }
}
