using FluentValidation;
 
namespace SocialHub.Application.Features.Users.Profile;
 
public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Bio).MaximumLength(500);
        RuleFor(x => x.Location).MaximumLength(100);
        RuleFor(x => x.Website)
            .MaximumLength(200)
            .Must(BeAValidAbsoluteUrl)
            .WithMessage("Website must be a valid absolute URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.Website));
    }
 
    private static bool BeAValidAbsoluteUrl(string? website) =>
        Uri.TryCreate(website, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}