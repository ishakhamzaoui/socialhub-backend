using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Policies;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Follow;
 
public sealed class GetFollowersQueryHandler : IQueryHandler<GetFollowersQuery, PagedUserListDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
 
    public GetFollowersQueryHandler(
        ICurrentUserService currentUserService,
        IUserProfileRepository userProfileRepository,
        IFollowRepository followRepository,
        IUserBlockRepository userBlockRepository)
    {
        _currentUserService = currentUserService;
        _userProfileRepository = userProfileRepository;
        _followRepository = followRepository;
        _userBlockRepository = userBlockRepository;
    }
 
    public async Task<Result<PagedUserListDto>> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<PagedUserListDto>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var targetProfile = await _userProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (targetProfile is null)
        {
            return Result.Failure<PagedUserListDto>(Error.NotFound("User.NotFound", "This user could not be found."));
        }
 
        var access = await ProfileAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, targetProfile, requesterId, cancellationToken);
        switch (access)
        {
            case ProfileAccessResult.Blocked:
                return Result.Failure<PagedUserListDto>(Error.NotFound("User.NotFound", "This user could not be found."));
            case ProfileAccessResult.Denied:
                return Result.Failure<PagedUserListDto>(Error.Forbidden("Profile.Private", "This user's followers are not visible to you."));
        }
 
        var excludeIds = await GetBlockedRelationshipIdsAsync(requesterId, cancellationToken);
        var (ids, total) = await _followRepository.GetFollowerIdsAsync(request.UserId, request.Page, request.PageSize, excludeIds, cancellationToken);
        var summaries = await ResolveSummariesInOrderAsync(ids, cancellationToken);
 
        return Result.Success(new PagedUserListDto(summaries, total, request.Page, request.PageSize));
    }
 
    private async Task<IReadOnlyList<Guid>> GetBlockedRelationshipIdsAsync(Guid requesterId, CancellationToken cancellationToken)
    {
        var blockedByMe = await _userBlockRepository.GetBlockedUserIdsAsync(requesterId, cancellationToken);
        var blockedMe = await _userBlockRepository.GetBlockedByUserIdsAsync(requesterId, cancellationToken);
        return blockedByMe.Concat(blockedMe).Distinct().ToList();
    }
 
    private async Task<IReadOnlyList<UserSummaryDto>> ResolveSummariesInOrderAsync(IReadOnlyList<Guid> orderedIds, CancellationToken cancellationToken)
    {
        if (orderedIds.Count == 0)
        {
            return Array.Empty<UserSummaryDto>();
        }
 
        var profiles = await _userProfileRepository.GetByUserIdsAsync(orderedIds, cancellationToken);
        var profilesById = profiles.ToDictionary(p => p.UserId);
 
        return orderedIds
            .Where(profilesById.ContainsKey)
            .Select(id => UserSummaryDto.From(profilesById[id]))
            .ToList();
    }
}