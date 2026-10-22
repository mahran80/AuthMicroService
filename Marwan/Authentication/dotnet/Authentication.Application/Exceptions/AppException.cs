using System.Net;

namespace Authentication.Application.Exceptions
{
    public abstract class AppException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        protected AppException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
            : base(message)
        {
            StatusCode = statusCode;
        }

        protected AppException(string message, Exception innerException, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) 
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
