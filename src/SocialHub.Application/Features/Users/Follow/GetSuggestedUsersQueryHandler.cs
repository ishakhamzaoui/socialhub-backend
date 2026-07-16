using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Follow;
 
public sealed class GetSuggestedUsersQueryHandler : IQueryHandler<GetSuggestedUsersQuery, IReadOnlyList<UserSummaryDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUserBlockRepository _userBlockRepository;
 
    public GetSuggestedUsersQueryHandler(
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
 
    public async Task<Result<IReadOnlyList<UserSummaryDto>>> Handle(GetSuggestedUsersQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<IReadOnlyList<UserSummaryDto>>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var blockedByMe = await _userBlockRepository.GetBlockedUserIdsAsync(requesterId, cancellationToken);
        var blockedMe = await _userBlockRepository.GetBlockedByUserIdsAsync(requesterId, cancellationToken);
        var excludeIds = blockedByMe.Concat(blockedMe).Distinct().ToList();
 
        var ids = await _followRepository.GetSuggestedUserIdsAsync(requesterId, request.Limit, excludeIds, cancellationToken);
        if (ids.Count == 0)
        {
            return Result.Success<IReadOnlyList<UserSummaryDto>>(Array.Empty<UserSummaryDto>());
        }
 
        var profiles = await _userProfileRepository.GetByUserIdsAsync(ids, cancellationToken);
        var profilesById = profiles.ToDictionary(p => p.UserId);
 
        var summaries = ids
            .Where(profilesById.ContainsKey)
            .Select(id => UserSummaryDto.From(profilesById[id]))
            .ToList();
 
        return Result.Success<IReadOnlyList<UserSummaryDto>>(summaries);
    }
}