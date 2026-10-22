using Authentication.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Application.Interfaces
{
    public interface IMicrosoftAuthService
    {
        Task<ExternalUserInfo?> ValidateAsync(string credential);
    }
}