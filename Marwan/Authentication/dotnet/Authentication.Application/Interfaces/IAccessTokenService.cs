using Authentication.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Authentication.Application.Interfaces
{
    public interface IAccessTokenService
    {
        Task<(string Token, DateTimeOffset ExpiresAt)> GenerateAsync(ApplicationUser user);
    }
}
