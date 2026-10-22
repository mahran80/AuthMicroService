
using Authentication.Infrastructure.Social;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Authentication.Infrastructure.Tests.TestDoubles
{
    internal static class FacebookAuthServiceTestDoubles
    {
        private static readonly string ConfigurationFilePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "appsettings.Test.json");

        private static readonly IConfiguration Configuration =
            new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(
                    "appsettings.Test.json",
                    optional: false,
                    reloadOnChange: false)
                .Build();

        public static string GetValue(string key)
        {
            return Configuration[
                $"FacebookTestData:{key.Replace(".", ":")}"]
                ?? throw new InvalidOperationException(
                    $"Missing test configuration: {key}");
        }

        public static string GetJson(string key)
        {
            using var document = JsonDocument.Parse(
                File.ReadAllText(ConfigurationFilePath));

            JsonElement element = document.RootElement;

            foreach (string part in
                     ("FacebookTestData." + key).Split('.'))
            {
                if (element.ValueKind != JsonValueKind.Object ||
                    !element.TryGetProperty(part, out element))
                {
                    throw new InvalidOperationException(
                        $"Missing test JSON: {key}");
                }
            }

            return element.ValueKind == JsonValueKind.String
                ? element.GetString()!
                : element.GetRawText();
        }

        public static FacebookAuthService CreateService(
            FakeHttpMessageHandler handler)
        {
            return new FacebookAuthService(
                new HttpClient(handler),
                NullLogger<FacebookAuthService>.Instance);
        }

        public static HttpResponseMessage JsonResponse(
            HttpStatusCode statusCode,
            string json)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json")
            };
        }

        public static FakeHttpMessageHandler CreateHandler(
            HttpStatusCode statusCode,
            string json)
        {
            return new FakeHttpMessageHandler(
                _ => JsonResponse(statusCode, json));
        }
    }
}