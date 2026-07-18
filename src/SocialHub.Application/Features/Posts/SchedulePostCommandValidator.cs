using FluentValidation;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class SchedulePostCommandValidator : AbstractValidator<SchedulePostCommand>
{
    public SchedulePostCommandValidator()
    {
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.ScheduledForUtc).GreaterThan(_ => DateTime.UtcNow);
    }
}