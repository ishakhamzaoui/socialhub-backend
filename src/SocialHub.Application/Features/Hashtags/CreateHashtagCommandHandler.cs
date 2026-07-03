using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Shared;
 
namespace SocialHub.Application.Features.Hashtags;
 
public sealed class CreateHashtagCommandHandler : ICommandHandler<CreateHashtagCommand, HashtagDto>
{
    private readonly IHashtagRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
 
    public CreateHashtagCommandHandler(IHashtagRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<HashtagDto>> Handle(CreateHashtagCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Tag.Trim().TrimStart('#').ToUpperInvariant();
 
        var existing = await _repository.GetByNormalizedTagAsync(normalized, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<HashtagDto>(
                Error.Conflict("Hashtag.AlreadyExists", $"The hashtag '{request.Tag}' already exists."));
        }
 
        var hashtag = Hashtag.Create(request.Tag);
        await _repository.AddAsync(hashtag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success(new HashtagDto(hashtag.Id, hashtag.Tag, hashtag.UsageCount));
    }
}