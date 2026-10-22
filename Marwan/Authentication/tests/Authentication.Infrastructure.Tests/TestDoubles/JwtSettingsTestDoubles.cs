using Authentication.Domain.Entities;
using Authentication.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Authentication.Infrastructure.Tests.TestDoubles
{
    public static class JwtSettingsTestDoubles
    {
        private static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(
                    "appsettings.Test.json",
                    optional: false,
                    reloadOnChange: false)
                .Build();
        }

        public static IOptions<JwtSettings> CreateSettings()
        {
            var settings = LoadConfiguration()
                .GetSection("JwtSettings")
                .Get<JwtSettings>();

            if (settings is null)
            {
                throw new InvalidOperationException(
                    "JwtSettings section was not found in appsettings.Test.json.");
            }

            return Options.Create(settings);
        }

        public static ApplicationUser CreateUser()
        {
            var section = LoadConfiguration()
                .GetSection("AccessTokenTestData:User");

            return new ApplicationUser
            {
                Email = section["Email"],
                UserName = section["UserName"]
            };
        }

        public static IList<string> CreateRoles()
        {
            return LoadConfiguration()
                .GetSection("AccessTokenTestData:Roles")
                .GetChildren()
                .Select(role => role.Value)
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role!)
                .ToList();
        }
    }
}