using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Application.DTOs
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
