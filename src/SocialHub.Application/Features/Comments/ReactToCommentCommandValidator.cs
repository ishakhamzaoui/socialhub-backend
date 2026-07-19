using FluentValidation;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class ReactToCommentCommandValidator : AbstractValidator<ReactToCommentCommand>
{
    public ReactToCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}