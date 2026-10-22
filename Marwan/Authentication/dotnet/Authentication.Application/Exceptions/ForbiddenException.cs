using System.Net;

namespace Authentication.Application.Exceptions
{
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message) 
            : base(message, HttpStatusCode.Forbidden)
        {
        }
    }
}
