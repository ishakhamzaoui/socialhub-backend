using FluentValidation;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class ReportCommentCommandValidator : AbstractValidator<ReportCommentCommand>
{
    public ReportCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
        RuleFor(x => x.Reason).IsInEnum();
        RuleFor(x => x.Details).MaximumLength(1000);
    }
}