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

    public static readonly Error PhoneAlreadyInUse = Error.Conflict(
        "Identity.PhoneAlreadyInUse",
        "The specified phone number is already in use.");

    public static readonly Error PhoneNotFound = Error.NotFound(
        "Identity.PhoneNotFound",
        "No account was found with the specified phone number.");

    public static readonly Error InvalidOrExpiredOtp = Error.Validation(
        "Identity.InvalidOrExpiredOtp",
        "The verification code is invalid, expired, or has exceeded maximum attempts.");

    public static readonly Error InvalidOrExpiredResetToken = Error.Validation(
        "Identity.InvalidOrExpiredResetToken",
        "The password reset token is invalid or has expired.");

    public static readonly Error NotificationDeliveryFailed = Error.Failure(
        "Identity.NotificationDeliveryFailed",
        "Failed to deliver notification message. Please try again later.");

    public static Error UnsupportedSocialProvider(string provider) => Error.Validation(
        "Identity.UnsupportedSocialProvider",
        $"The social authentication provider '{provider}' is not supported.");

    public static Error SocialProviderNotConfigured(string provider) => Error.Failure(
        "Identity.SocialProviderNotConfigured",
        $"The social authentication provider '{provider}' is not configured on the server.");

    public static Error InvalidSocialCredential(string provider) => Error.Unauthorized(
        "Identity.InvalidSocialCredential",
        $"The provided {provider} credentials or authorization code is invalid.");

    public static Error SocialEmailRequired(string provider) => Error.Validation(
        "Identity.SocialEmailRequired",
        $"The {provider} account did not provide a verified email address.");
}
