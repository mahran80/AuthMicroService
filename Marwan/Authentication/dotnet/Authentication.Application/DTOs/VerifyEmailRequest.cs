using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Application.DTOs
{

    public class VerifyEmailRequest
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
