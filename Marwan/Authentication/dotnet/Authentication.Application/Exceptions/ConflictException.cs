using System.Net;

namespace Authentication.Application.Exceptions
{
    public class ConflictException : AppException
    {
        public ConflictException(string message) 
            : base(message, HttpStatusCode.Conflict)
        {
        }
    }
}
