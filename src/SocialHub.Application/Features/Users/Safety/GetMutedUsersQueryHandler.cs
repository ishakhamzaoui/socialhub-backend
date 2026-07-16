using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Application.Features.Users.Follow;
 
namespace SocialHub.Application.Features.Users.Safety;
 
public sealed class GetMutedUsersQueryHandler : IQueryHandler<GetMutedUsersQuery, IReadOnlyList<UserSummaryDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserMuteRepository _userMuteRepository;
    private readonly IUserProfileRepository _userProfileRepository;
 
    public GetMutedUsersQueryHandler(
        ICurrentUserService currentUserService,
        IUserMuteRepository userMuteRepository,
        IUserProfileRepository userProfileRepository)
    {
        _currentUserService = currentUserService;
        _userMuteRepository = userMuteRepository;
        _userProfileRepository = userProfileRepository;
    }
 
    public async Task<Result<IReadOnlyList<UserSummaryDto>>> Handle(GetMutedUsersQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure<IReadOnlyList<UserSummaryDto>>(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var ids = await _userMuteRepository.GetMutedUserIdsAsync(requesterId, cancellationToken);
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