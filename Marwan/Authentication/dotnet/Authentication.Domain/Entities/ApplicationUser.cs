using Authentication.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;

namespace Authentication.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserStatus Status { get; set; } = UserStatus.Pending;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    }
}
