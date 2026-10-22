namespace Users.Domain.Errors;

using SharedKernel.Application;

public static class UsersErrors
{
    public static readonly Error ProfileNotFound = Error.NotFound(
        "Users.ProfileNotFound",
        "The requested user profile was not found.");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        "Users.EmailAlreadyExists",
        "A user profile with this email address already exists.");

    public static readonly Error PhoneAlreadyExists = Error.Conflict(
        "Users.PhoneAlreadyExists",
        "A user profile with this phone number already exists.");

    public static readonly Error ProfileAlreadyDeleted = Error.Conflict(
        "Users.ProfileAlreadyDeleted",
        "The user profile has already been deleted.");
}
