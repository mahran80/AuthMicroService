using FluentValidation;

namespace Identity.Application.Features.ResendOtp;

public sealed class ResendOtpValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpValidator()
    {
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

    private static bool HaveValidContact(ResendOtpCommand command)
    {
        bool hasEmail = !string.IsNullOrWhiteSpace(command.Email);
        bool hasPhone = !string.IsNullOrWhiteSpace(command.CountryCode) && !string.IsNullOrWhiteSpace(command.PhoneNumber);
        return hasEmail || hasPhone;
    }
}
