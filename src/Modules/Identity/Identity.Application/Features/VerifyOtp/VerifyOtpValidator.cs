using FluentValidation;

namespace Identity.Application.Features.VerifyOtp;

public sealed class VerifyOtpValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.");

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Purpose is required.");

        RuleFor(x => x)
            .Must(HaveValidContact)
            .WithMessage("Either a valid email address or country code and phone number must be provided.");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email format is invalid.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.CountryCode)
                .NotEmpty().WithMessage("Country code is required when phone number is provided.");
        });
    }

    private static bool HaveValidContact(VerifyOtpCommand command)
    {
        bool hasEmail = !string.IsNullOrWhiteSpace(command.Email);
        bool hasPhone = !string.IsNullOrWhiteSpace(command.CountryCode) && !string.IsNullOrWhiteSpace(command.PhoneNumber);
        return hasEmail || hasPhone;
    }
}
