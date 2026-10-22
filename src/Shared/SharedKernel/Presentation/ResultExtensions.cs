using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application;

namespace SharedKernel.Presentation;

public static class ResultExtensions
{
    public static ObjectResult ToProblemDetails(this Result result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot generate ProblemDetails from a successful result.");
        }

        var error = result.Error;

        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var title = error.Type switch
        {
            ErrorType.Validation => "Validation Failure",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            _ => "Internal Server Error"
        };

        var type = error.Type switch
        {
            ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = error.Description,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = error.Code;

        if (error.ValidationErrors is not null && error.ValidationErrors.Count > 0)
        {
            problemDetails.Extensions["errors"] = error.ValidationErrors;
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }
}
