using FluentValidation;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.Content).MaximumLength(5000);
        RuleFor(x => x.Visibility).IsInEnum();
    }
}