using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Application.Common.Specifications;
using DomainRefreshToken = SocialHub.Domain.Users.RefreshToken;
 
namespace SocialHub.Application.Features.Authentication.Sessions;
 
public sealed class GetActiveSessionsQueryHandler : IQueryHandler<GetActiveSessionsQuery, IReadOnlyList<SessionDto>>
{
    private readonly IRepository<DomainRefreshToken, Guid> _refreshTokens;
    private readonly ICurrentUserService _currentUserService;
 
    public GetActiveSessionsQueryHandler(IRepository<DomainRefreshToken, Guid> refreshTokens, ICurrentUserService currentUserService)
    {
        _refreshTokens = refreshTokens;
        _currentUserService = currentUserService;
    }
 
    public async Task<Result<IReadOnlyList<SessionDto>>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null || !Guid.TryParse(_currentUserService.UserId, out var userId))
        {
            return Result.Failure<IReadOnlyList<SessionDto>>(Error.Unauthorized("Auth.NotAuthenticated", "Not authenticated."));
        }
 
        var tokens = await _refreshTokens.ListAsync(new ActiveRefreshTokensByUserIdSpecification(userId), cancellationToken);
 
        IReadOnlyList<SessionDto> sessions = tokens
            .Select(t => new SessionDto(t.Id, t.DeviceName, t.CreatedByIp, t.CreatedAtUtc, t.ExpiresAtUtc))
            .ToList();
 
        return Result.Success(sessions);
    }
}