using Authentication.Infrastructure.Email;
using Authentication.Infrastructure.Tests.TestDoubles;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.Infrastructure.Tests.EmailTests
{
    public class SmtpEmailServiceTests
    {
        [Fact]
        public async Task SendEmailAsync_EmptyRecipient_ShouldThrowParseException()
        {
            // Arrange
            var settings = Options.Create(new EmailSettings
            {
                Host = "localhost",
                Port = 587,
                EnableSsl = true,
                FromName = "Test App",
                FromEmail = "sender@example.com"
            });

            var service = new SmtpEmailService(settings);

            // Act & Assert
            await Assert.ThrowsAsync<ParseException>(() =>
                service.SendEmailAsync(
                    "",
                    "Test Subject",
                    "<p>Test Body</p>"));
        }

        [Fact]
        public async Task SendEmailAsync_WhitespaceRecipient_ShouldThrowParseException()
        {
            // Arrange
            var settings = SmtpEmailServiceTestDoubles.CreateSettings();

            var service = new SmtpEmailService(settings);

            // Act & Assert
            await Assert.ThrowsAsync<ParseException>(() =>
                service.SendEmailAsync(
                    "   ",
                    "Test Subject",
                    "<p>Test Body</p>"));
        }
    }
}