using System.Net;

namespace Authentication.Application.Exceptions
{
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) 
            : base(message, HttpStatusCode.Unauthorized)
        {
        }

        public UnauthorizedException(string message, Exception innerException) 
            : base(message, innerException, HttpStatusCode.Unauthorized)
        {
        }
    }
}
