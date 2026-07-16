using FluentValidation;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed class UpdateThemePreferenceCommandValidator : AbstractValidator<UpdateThemePreferenceCommand>
{
    public UpdateThemePreferenceCommandValidator()
    {
        RuleFor(x => x.Theme).IsInEnum();
    }
}