using System.Net;

namespace Authentication.Application.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) 
            : base(message, HttpStatusCode.NotFound)
        {
        }
    }
}
