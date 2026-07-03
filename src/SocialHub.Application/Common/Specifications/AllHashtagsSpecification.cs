using SocialHub.Domain.Shared;
 
namespace SocialHub.Application.Common.Specifications;
 
public sealed class AllHashtagsSpecification : BaseSpecification<Hashtag>
{
    public AllHashtagsSpecification()
    {
        ApplyOrderBy(h => h.Tag);
    }
}