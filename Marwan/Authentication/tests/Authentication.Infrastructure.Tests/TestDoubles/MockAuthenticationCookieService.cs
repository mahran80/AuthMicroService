using Authentication.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Infrastructure.Tests.TestDoubles
{
    internal class MockAuthenticationCookieService
    {
        public Mock<IHttpContextAccessor> HttpContextAccessor { get; }
        public Mock<HttpContext> HttpContext { get; }
        public Mock<HttpResponse> HttpResponse { get; }
        public Mock<IResponseCookies> ResponseCookies { get; }
        public AuthenticationCookieService Service { get; }

        public MockAuthenticationCookieService()
        {
            //creating mock for coockies
            ResponseCookies = new Mock<IResponseCookies>();
            HttpResponse = new Mock<HttpResponse>();
            HttpResponse.SetupGet(r=>r.Cookies).Returns(ResponseCookies.Object);
            
            HttpContext = new Mock<HttpContext>();
            HttpContext.SetupGet(c=>c.Response).Returns(HttpResponse.Object);

            HttpContextAccessor = new Mock<IHttpContextAccessor>();
            HttpContextAccessor.SetupGet(a=>a.HttpContext).Returns(HttpContext.Object);
            Service = new AuthenticationCookieService(HttpContextAccessor.Object);
        }
    }
}
