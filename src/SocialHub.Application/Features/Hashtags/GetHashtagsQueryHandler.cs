using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Application.Common.Specifications;
 
namespace SocialHub.Application.Features.Hashtags;
 
public sealed class GetHashtagsQueryHandler : IQueryHandler<GetHashtagsQuery, IReadOnlyList<HashtagDto>>
{
    private readonly IHashtagRepository _repository;
 
    public GetHashtagsQueryHandler(IHashtagRepository repository)
    {
        _repository = repository;
    }
 
    public async Task<Result<IReadOnlyList<HashtagDto>>> Handle(GetHashtagsQuery request, CancellationToken cancellationToken)
    {
        var hashtags = await _repository.ListAsync(new AllHashtagsSpecification(), cancellationToken);
 
        IReadOnlyList<HashtagDto> dtos = hashtags
            .Select(h => new HashtagDto(h.Id, h.Tag, h.UsageCount))
            .ToList();
 
        return Result.Success(dtos);
    }
}