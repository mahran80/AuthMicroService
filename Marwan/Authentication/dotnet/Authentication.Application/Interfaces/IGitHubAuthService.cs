using Authentication.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Application.Interfaces
{
    public interface IGitHubAuthService
    {
        Task<ExternalUserInfo?> ValidateAsync(string Credential);
    }
}
