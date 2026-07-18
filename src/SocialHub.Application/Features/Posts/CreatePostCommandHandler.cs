using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
 
namespace SocialHub.Application.Features.Posts;
 
public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, PostDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPostRepository _postRepository;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IHashtagRepository _hashtagRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public CreatePostCommandHandler(
        ICurrentUserService currentUserService,
        IPostRepository postRepository,
        IMediaAssetRepository mediaAssetRepository,
        IHashtagRepository hashtagRepository,
        IUserProfileRepository userProfileRepository,
        IUserBlockRepository userBlockRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _postRepository = postRepository;
        _mediaAssetRepository = mediaAssetRepository;
        _hashtagRepository = hashtagRepository;
        _userProfileRepository = userProfileRepository;
        _userBlockRepository = userBlockRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<PostDto>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var authorId))
        {
            return Result.Failure<PostDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        if (request.Type == PostType.Quote)
        {
            var quoteCheck = await ValidateQuoteTargetAsync(authorId, request.OriginalPostId!.Value, cancellationToken);
            if (quoteCheck is not null)
            {
                return Result.Failure<PostDto>(quoteCheck);
            }
        }
 
        Post post;
        try
        {
            post = request.DesiredStatus switch
            {
                PostStatus.Draft => Post.CreateDraft(authorId, request.Content, request.Visibility, request.Type, request.OriginalPostId),
                PostStatus.Scheduled => Post.CreateScheduled(authorId, request.Content, request.Visibility, request.ScheduledForUtc!.Value, request.Type, request.OriginalPostId),
                _ => Post.CreatePublished(authorId, request.Content, request.Visibility, request.Type, request.OriginalPostId),
            };
        }
        catch (ArgumentException ex)
        {
            // Belt-and-braces: the validator already rejects these shapes,
            // but Post's own factories re-validate Type/OriginalPostId
            // consistency and would otherwise surface as an unhandled 500.
            return Result.Failure<PostDto>(Error.Validation("Post.Invalid", ex.Message));
        }
 
        await _postRepository.AddAsync(post, cancellationToken);
 
        if (request.MediaAssetIds is { Count: > 0 })
        {
            var mediaError = await AttachMediaAsync(post, authorId, request.MediaAssetIds, cancellationToken);
            if (mediaError is not null)
            {
                return Result.Failure<PostDto>(mediaError);
            }
        }
 
        if (request.HashtagTags is { Count: > 0 })
        {
            await AttachHashtagsAsync(post, request.HashtagTags, cancellationToken);
        }
 
        if (request.MentionedUserIds is { Count: > 0 })
        {
            await AttachMentionsAsync(post, request.MentionedUserIds, cancellationToken);
        }
 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        var dto = await PostDtoFactory.CreateAsync(post, _mediaAssetRepository, _hashtagRepository, cancellationToken);
        return Result.Success(dto);
    }
 
    private async Task<Error?> ValidateQuoteTargetAsync(Guid authorId, Guid originalPostId, CancellationToken cancellationToken)
    {
        var original = await _postRepository.GetByIdAsync(originalPostId, cancellationToken);
 
        // NotFound (not a more specific error) whether the post genuinely
        // doesn't exist, isn't published yet, or a block exists between the
        // two authors — same non-leaking-existence convention
        // ProfileAccessPolicy already established in Phase 5.
        if (original is null || original.Status != PostStatus.Published)
        {
            return Error.NotFound("Post.NotFound", "The post you're trying to quote doesn't exist.");
        }
 
        if (await _userBlockRepository.IsBlockedEitherDirectionAsync(authorId, original.AuthorId, cancellationToken))
        {
            return Error.NotFound("Post.NotFound", "The post you're trying to quote doesn't exist.");
        }
 
        return null;
    }
 
    private async Task<Error?> AttachMediaAsync(Post post, Guid authorId, IReadOnlyList<Guid> mediaAssetIds, CancellationToken cancellationToken)
    {
        for (var i = 0; i < mediaAssetIds.Count; i++)
        {
            var asset = await _mediaAssetRepository.GetByIdForOwnerAsync(mediaAssetIds[i], authorId, cancellationToken);
            if (asset is null)
            {
                return Error.NotFound("Media.NotFound", "Media not found.");
            }
 
            post.AttachMedia(asset.Id, order: i);
        }
 
        return null;
    }
 
    private async Task AttachHashtagsAsync(Post post, IReadOnlyList<string> hashtagTags, CancellationToken cancellationToken)
    {
        foreach (var rawTag in hashtagTags)
        {
            var normalized = rawTag.Trim().TrimStart('#').ToUpperInvariant();
            if (normalized.Length == 0)
            {
                continue;
            }
 
            var hashtag = await _hashtagRepository.GetByNormalizedTagAsync(normalized, cancellationToken);
            if (hashtag is null)
            {
                hashtag = Hashtag.Create(rawTag);
                await _hashtagRepository.AddAsync(hashtag, cancellationToken);
            }
 
            hashtag.IncrementUsage();
            post.AddHashtag(hashtag.Id);
        }
    }
 
    private async Task AttachMentionsAsync(Post post, IReadOnlyList<Guid> mentionedUserIds, CancellationToken cancellationToken)
    {
        foreach (var userId in mentionedUserIds)
        {
            // Silently skip anyone with no profile rather than failing the
            // whole command over a stale client-side selection (see this
            // script's header notes).
            var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
            if (profile is not null)
            {
                post.AddMention(userId);
            }
        }
    }
}