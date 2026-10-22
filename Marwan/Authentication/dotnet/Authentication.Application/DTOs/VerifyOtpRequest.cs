using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Application.DTOs
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}
