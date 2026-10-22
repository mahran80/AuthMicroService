using System.Net;

namespace Authentication.Application.Exceptions
{
    public class EmailServiceException : AppException
    {
        public EmailServiceException(string message, Exception innerException) 
            : base(message, innerException, HttpStatusCode.BadGateway)
        {
        }

        public EmailServiceException(string message) 
            : base(message, HttpStatusCode.BadGateway)
        {
        }
    }
}
