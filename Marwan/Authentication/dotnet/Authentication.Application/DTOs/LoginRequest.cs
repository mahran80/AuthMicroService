using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Application.DTOs
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
