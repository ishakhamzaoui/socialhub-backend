using FluentValidation;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed class UpdatePrivacySettingsCommandValidator : AbstractValidator<UpdatePrivacySettingsCommand>
{
    public UpdatePrivacySettingsCommandValidator()
    {
        RuleFor(x => x.Visibility).IsInEnum();
    }
}