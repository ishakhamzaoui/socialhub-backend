using FluentValidation;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed class UpdateLanguagePreferenceCommandValidator : AbstractValidator<UpdateLanguagePreferenceCommand>
{
    public UpdateLanguagePreferenceCommandValidator()
    {
        RuleFor(x => x.Language)
            .NotEmpty()
            .Matches("^[a-zA-Z]{2}$")
            .WithMessage("Language must be a two-letter ISO 639-1 code, e.g. 'en' or 'fr'.");
    }
}