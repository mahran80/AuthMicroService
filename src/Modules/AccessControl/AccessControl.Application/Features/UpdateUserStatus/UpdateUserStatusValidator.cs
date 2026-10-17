using FluentValidation;

namespace AccessControl.Application.Features.UpdateUserStatus;

public sealed class UpdateUserStatusValidator : AbstractValidator<UpdateUserStatusCommand>
{
    private static readonly string[] ValidStatuses = { "Active", "Suspended", "Deactivated" };

    public UpdateUserStatusValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Status must be one of: Active, Suspended, Deactivated.");
    }
}
