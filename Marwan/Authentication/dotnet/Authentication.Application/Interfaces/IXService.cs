using Authentication.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Application.Interfaces
{
    public interface IXService
    {
        Task<ExternalUserInfo?> ValidateAsync(string credential);
    }
}
