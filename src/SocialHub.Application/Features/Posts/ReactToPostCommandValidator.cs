using FluentValidation;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class ReactToPostCommandValidator : AbstractValidator<ReactToPostCommand>
{
    public ReactToPostCommandValidator()
    {
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}