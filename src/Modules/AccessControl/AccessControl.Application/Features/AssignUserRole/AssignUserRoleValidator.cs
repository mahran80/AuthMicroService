using FluentValidation;

namespace AccessControl.Application.Features.AssignUserRole;

public sealed class AssignUserRoleValidator : AbstractValidator<AssignUserRoleCommand>
{
    public AssignUserRoleValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required.");
    }
}
