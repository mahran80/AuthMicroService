namespace Identity.Domain.Errors;

using SharedKernel.Application;

public static class IdentityErrors
{
    public static readonly Error EmailAlreadyInUse = Error.Conflict(
        "Identity.EmailAlreadyInUse",
        "The specified email address is already registered.");

    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Identity.InvalidCredentials",
        "The provided email or password is incorrect.");

    public static readonly Error AccountNotActive = Error.Forbidden(
        "Identity.AccountNotActive",
        "The user account is not active.");

    public static readonly Error InvalidRefreshToken = Error.Unauthorized(
        "Identity.InvalidRefreshToken",
        "The refresh token is invalid, expired, or has been revoked.");

    public static readonly Error UserNotFound = Error.NotFound(
        "Identity.UserNotFound",
        "The requested user was not found.");
}
