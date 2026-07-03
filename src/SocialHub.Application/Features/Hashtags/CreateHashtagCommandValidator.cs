using FluentValidation;
 
namespace SocialHub.Application.Features.Hashtags;
 
public sealed class CreateHashtagCommandValidator : AbstractValidator<CreateHashtagCommand>
{
    public CreateHashtagCommandValidator()
    {
        RuleFor(x => x.Tag)
            .NotEmpty()
            .MaximumLength(100);
    }
}