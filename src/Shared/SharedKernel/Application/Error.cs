namespace SharedKernel.Application;

public sealed record Error(
    string Code,
    string Description,
    ErrorType Type,
    IDictionary<string, string[]>? ValidationErrors = null)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);

    public static Error Validation(
        string code,
        string description,
        IDictionary<string, string[]>? validationErrors = null) =>
        new(code, description, ErrorType.Validation, validationErrors);
}
