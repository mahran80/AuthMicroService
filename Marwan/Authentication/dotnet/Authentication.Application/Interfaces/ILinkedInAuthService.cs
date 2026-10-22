
using Authentication.Application.DTOs;
using System.Threading.Tasks;

namespace Authentication.Application.Interfaces
{
    public interface ILinkedInAuthService
    {
        Task<ExternalUserInfo?> ValidateAsync(string credential);
    }
}
