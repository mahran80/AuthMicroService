using Authentication.Infrastructure.Social;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Authentication.Infrastructure.Tests.TestDoubles
{
    public static class GoogleAuthServiceTestDoubles
    {
        private static IConfiguration LoadConfiguration(
            bool includeClientId = true)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(
                    "appsettings.Test.json",
                    optional: false,
                    reloadOnChange: false)
                .Build();

            var values = new Dictionary<string, string?>();

            if (includeClientId)
            {
                values["Google:ClientId"] =
                    configuration["Google:ClientId"];
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }

        public static GoogleAuthService CreateService(
            bool includeClientId = true)
        {
            var configuration = LoadConfiguration(includeClientId);

            return new GoogleAuthService(
                configuration,
                NullLogger<GoogleAuthService>.Instance);
        }
    }
}