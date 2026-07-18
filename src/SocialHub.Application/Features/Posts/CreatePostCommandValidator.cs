using FluentValidation;
using SocialHub.Domain.Posts;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    // Soft caps — not dictated by the specification, chosen as reasonable
    // defaults (flagged in this script's delivery notes). Adjust directly
    // if product requirements turn out to differ.
    private const int MaxContentLength = 5000;
    private const int MaxMediaAttachments = 10;
    private const int MaxHashtags = 10;
    private const int MaxMentions = 50;
 
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Content).MaximumLength(MaxContentLength);
 
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Visibility).IsInEnum();
 
        RuleFor(x => x.DesiredStatus)
            .Must(status => status is PostStatus.Draft or PostStatus.Scheduled or PostStatus.Published)
            .WithMessage("A post can only be created as Draft, Scheduled, or Published.");
 
        RuleFor(x => x.OriginalPostId)
            .NotNull()
            .When(x => x.Type == PostType.Quote)
            .WithMessage("A quote post must reference an original post.");
 
        RuleFor(x => x.OriginalPostId)
            .Null()
            .When(x => x.Type == PostType.Original)
            .WithMessage("An original post cannot reference another post.");
 
        RuleFor(x => x.ScheduledForUtc)
            .NotNull()
            .GreaterThan(_ => DateTime.UtcNow)
            .When(x => x.DesiredStatus == PostStatus.Scheduled)
            .WithMessage("A scheduled post requires a future ScheduledForUtc.");
 
        RuleFor(x => x.ScheduledForUtc)
            .Null()
            .When(x => x.DesiredStatus != PostStatus.Scheduled)
            .WithMessage("ScheduledForUtc is only meaningful when DesiredStatus is Scheduled.");
 
        RuleFor(x => x)
            .Must(HaveContentOrMedia)
            .WithMessage("A post needs Content, at least one media attachment, or both.");
 
        RuleFor(x => x.MediaAssetIds)
            .Must(ids => ids is null || ids.Count <= MaxMediaAttachments)
            .WithMessage($"A post cannot have more than {MaxMediaAttachments} media attachments.");
 
        RuleFor(x => x.HashtagTags)
            .Must(tags => tags is null || tags.Count <= MaxHashtags)
            .WithMessage($"A post cannot have more than {MaxHashtags} hashtags.");
 
        RuleForEach(x => x.HashtagTags).NotEmpty().MaximumLength(100);
 
        RuleFor(x => x.MentionedUserIds)
            .Must(ids => ids is null || ids.Count <= MaxMentions)
            .WithMessage($"A post cannot mention more than {MaxMentions} users.");
    }
 
    private static bool HaveContentOrMedia(CreatePostCommand command) =>
        !string.IsNullOrWhiteSpace(command.Content) || (command.MediaAssetIds?.Count ?? 0) > 0;
}