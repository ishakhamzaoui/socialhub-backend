using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Users.Safety;
 
public sealed class UnblockUserCommandHandler : ICommandHandler<UnblockUserCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public UnblockUserCommandHandler(
        ICurrentUserService currentUserService,
        IUserBlockRepository userBlockRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userBlockRepository = userBlockRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var requesterId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "A valid authenticated user is required."));
        }
 
        var block = await _userBlockRepository.GetAsync(requesterId, request.TargetUserId, cancellationToken);
        if (block is null)
        {
            return Result.Failure(Error.NotFound("Block.NotFound", "You have not blocked this user."));
        }
 
        _userBlockRepository.Remove(block);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        return Result.Success();
    }
}