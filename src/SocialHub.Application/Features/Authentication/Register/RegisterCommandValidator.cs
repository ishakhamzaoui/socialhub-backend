using FluentValidation;
 
namespace SocialHub.Application.Features.Authentication.Register;
 
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
 
        // Mirrors the password policy configured in
        // SocialHub.Identity.DependencyInjection.AddIdentityInfrastructure —
        // duplicated deliberately so invalid passwords fail fast in the
        // pipeline's ValidationBehavior instead of round-tripping to
        // UserManager first.
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}