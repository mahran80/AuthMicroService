using System.Reflection;
using FluentValidation;
using MediatR;

namespace SharedKernel.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errorsDictionary = failures
            .GroupBy(
                f => f.PropertyName,
                f => f.ErrorMessage,
                (propertyName, errorMessages) => new
                {
                    Key = propertyName,
                    Values = errorMessages.Distinct().ToArray()
                })
            .ToDictionary(x => x.Key, x => x.Values);

        var validationError = Error.Validation(
            "Validation.Failure",
            "One or more validation errors occurred.",
            errorsDictionary);

        return CreateValidationResult<TResponse>(validationError);
    }

    private static TResult CreateValidationResult<TResult>(Error error)
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (TResult)(object)Result.Failure(error);
        }

        if (typeof(TResult).IsGenericType &&
            typeof(TResult).GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type valueType = typeof(TResult).GetGenericArguments()[0];

            MethodInfo? failureMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name == nameof(Result.Failure) &&
                    m.IsGenericMethodDefinition &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType == typeof(Error));

            if (failureMethod != null)
            {
                MethodInfo genericMethod = failureMethod.MakeGenericMethod(valueType);
                object? result = genericMethod.Invoke(null, new object[] { error });
                if (result != null)
                {
                    return (TResult)result;
                }
            }
        }

        throw new ValidationException(error.Description);
    }
}
