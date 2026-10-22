using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Infrastructure.Tests.TestDoubles
{
    internal static class MockUserManager
    {
        public static Mock<UserManager<ApplicationUser>> Create(IList<string>? roles = null)
        {
            //creating mock
            var userStore= new Mock<IUserStore<ApplicationUser>>();
            var mock = new Mock<UserManager<ApplicationUser>>(
              userStore.Object,
              null!,
              null!,
              null!,
              null!,
              null!,
              null!,
              null!,
              null!
            );
            //setup mock
            mock.Setup(x=>x.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(roles ?? new List<string>());
            return mock;
        }
    }
}
