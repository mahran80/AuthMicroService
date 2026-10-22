using FluentValidation;

namespace Identity.Application.Features.SocialLogin;

public sealed class SocialLoginValidator : AbstractValidator<SocialLoginCommand>
{
    private static readonly HashSet<string> AllowedProviders = new(StringComparer.OrdinalIgnoreCase)
    {
        "google",
        "facebook",
        "github",
        "linkedin",
        "microsoft"
    };

    public SocialLoginValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required.")
            .Must(provider => AllowedProviders.Contains(provider.Trim()))
            .WithMessage("Provider must be one of: google, facebook, github, linkedin, microsoft.");

        RuleFor(x => x.Credential)
            .NotEmpty().WithMessage("Credential or authorization code is required.");
    }
}
