using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.Tests.TestDoubles
{
    internal class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage>_responseFactory;

        public int RequestCount { get; private set; }

        public string? LastRequestUrl { get; private set; }

        public FakeHttpMessageHandler(
            Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RequestCount++;
            LastRequestUrl = request.RequestUri?.ToString();

            return Task.FromResult(_responseFactory(request));
        }
    }
}