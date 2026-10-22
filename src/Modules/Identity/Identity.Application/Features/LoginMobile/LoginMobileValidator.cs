using FluentValidation;

namespace Identity.Application.Features.LoginMobile;

public sealed class LoginMobileValidator : AbstractValidator<LoginMobileCommand>
{
    public LoginMobileValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(25).WithMessage("Phone number cannot exceed 25 characters.");

        RuleFor(x => x.CountryCode)
            .MaximumLength(10).WithMessage("Country code cannot exceed 10 characters.");
    }
}
