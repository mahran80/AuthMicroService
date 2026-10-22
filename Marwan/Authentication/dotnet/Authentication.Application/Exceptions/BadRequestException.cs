using System.Net;

namespace Authentication.Application.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message) 
            : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}
