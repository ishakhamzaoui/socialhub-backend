using FluentValidation;
 
namespace SocialHub.Application.Features.Comments;
 
public sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    private const int MaxContentLength = 2000;
    private const int MaxMentions = 50;
 
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(MaxContentLength);
        RuleFor(x => x.MentionedUserIds)
            .Must(ids => ids is null || ids.Count <= MaxMentions)
            .WithMessage($"A comment cannot mention more than {MaxMentions} users.");
    }
}