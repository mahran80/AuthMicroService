namespace AccessControl.Domain.Errors;

using SharedKernel.Application;

public static class RbacErrors
{
    public static readonly Error RoleAlreadyExists = Error.Conflict(
        "Rbac.RoleAlreadyExists",
        "A role with the specified name already exists.");

    public static readonly Error RoleNotFound = Error.NotFound(
        "Rbac.RoleNotFound",
        "The specified role was not found.");

    public static readonly Error SystemRoleModificationForbidden = Error.Forbidden(
        "Rbac.SystemRoleModificationForbidden",
        "System roles cannot have core permissions removed or be deleted.");

    public static readonly Error InvalidPermissionCodes = Error.Validation(
        "Rbac.InvalidPermissionCodes",
        "One or more specified permission codes do not exist in the catalog.");

    public static readonly Error UserNotFound = Error.NotFound(
        "Rbac.UserNotFound",
        "The specified user does not exist.");
}
