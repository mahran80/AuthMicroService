using FluentValidation;

namespace AccessControl.Application.Features.UpdateRolePermissions;

public sealed class UpdateRolePermissionsValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required.");

        RuleFor(x => x.PermissionCodes)
            .NotNull().WithMessage("Permission codes collection cannot be null.");
    }
}
