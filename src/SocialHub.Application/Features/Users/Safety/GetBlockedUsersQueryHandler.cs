using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Application.Features.Users.Follow;
 
namespace SocialHub.Application.Features.Users.Safety;
 
public sealed class GetBlockedUsersQueryHandler : IQueryHandler<GetBlockedUsersQuery, IReadOnlyList<UserSummaryDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUserProfileRepository _userProfileRepository;
 
    public GetBlockedUsersQueryHandler(
        ICurrentUserService currentUserService,
        IUserBlockRepository userBlockRepository,
        IUserProfileRepository userProfileRepository)
    {
        _currentUserService = currentUserService;
        _userBlockRepository = userBlockRepository;
        _userProfileRepository = userProfileRepository;
    }
 
    public async Task<Result<IReadOnlyList<UserSummaryDto>>> Handle(GetBlockedUsersQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<IReadOnlyList<UserSummaryDto>>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var ids = await _userBlockRepository.GetBlockedUserIdsAsync(requesterId, cancellationToken);
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