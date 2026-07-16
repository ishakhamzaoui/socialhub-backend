using FluentValidation;
 
namespace SocialHub.Application.Features.Users.Follow;
 
public sealed class GetFollowingQueryValidator : AbstractValidator<GetFollowingQuery>
{
    public GetFollowingQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}