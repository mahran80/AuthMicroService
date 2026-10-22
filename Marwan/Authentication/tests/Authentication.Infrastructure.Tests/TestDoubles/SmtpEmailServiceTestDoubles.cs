using Authentication.Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Authentication.Infrastructure.Tests.TestDoubles
{
    public static class SmtpEmailServiceTestDoubles
    {
        public static IOptions<EmailSettings> CreateSettings()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(
                    "appsettings.Test.json",
                    optional: false,
                    reloadOnChange: false)
                .Build();

            var services = new ServiceCollection();

            services.AddOptions<EmailSettings>()
                .Bind(configuration.GetSection("EmailSettings"));

            using var provider = services.BuildServiceProvider();

            return provider
                .GetRequiredService<IOptions<EmailSettings>>();
        }
    }
}