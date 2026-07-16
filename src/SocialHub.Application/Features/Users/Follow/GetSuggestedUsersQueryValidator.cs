using FluentValidation;
 
namespace SocialHub.Application.Features.Users.Follow;
 
public sealed class GetSuggestedUsersQueryValidator : AbstractValidator<GetSuggestedUsersQuery>
{
    public GetSuggestedUsersQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
    }
}