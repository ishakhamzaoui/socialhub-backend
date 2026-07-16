using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
using SocialHub.Domain.Users;
 
namespace SocialHub.Application.Features.Authentication.Register;
 
public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;
    private readonly IAppUrlProvider _appUrlProvider;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
 
    public RegisterCommandHandler(
        IIdentityService identityService,
        IEmailSender emailSender,
        IAppUrlProvider appUrlProvider,
        IUserProfileRepository userProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _emailSender = emailSender;
        _appUrlProvider = appUrlProvider;
        _userProfileRepository = userProfileRepository;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result<RegisterResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var createResult = await _identityService.CreateUserAsync(request.Email, request.Password, cancellationToken);
        if (createResult.IsFailure)
        {
            return Result.Failure<RegisterResponseDto>(createResult.Error);
        }
 
        // Phase 5 addition: every account gets a default UserProfile row
        // created eagerly here, at the one identifiable point of account
        // creation — deliberately NOT lazily on first profile read, which
        // would make GET /users/{id}/profile a side-effecting request when
        // looking up someone else's profile (e.g. from a followers list).
        // See the Phase 5 context doc for the full reasoning, and
        // ApplicationDbContextSeeder for the backfill this required for the
        // pre-existing dev admin account.
        var displayName = request.Email.Split('@')[0];
        var profile = UserProfile.CreateDefault(createResult.Value, displayName);
        await _userProfileRepository.AddAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        var tokenResult = await _identityService.GenerateEmailConfirmationTokenAsync(createResult.Value, cancellationToken);
        if (tokenResult.IsSuccess)
        {
            var confirmationUrl = _appUrlProvider.BuildEmailConfirmationUrl(createResult.Value, tokenResult.Value);
            var body = $"""
                <p>Welcome to SocialHub!</p>
                <p>Please confirm your email address by visiting the link below:</p>
                <p><a href="{confirmationUrl}">{confirmationUrl}</a></p>
                <p>(Dev note: the raw token is <code>{tokenResult.Value}</code> if you need to call
                POST /api/v1/auth/confirm-email directly, e.g. via Swagger, before a frontend exists.)</p>
                """;
 
            await _emailSender.SendAsync(request.Email, "Confirm your SocialHub account", body, cancellationToken);
        }
 
        return Result.Success(new RegisterResponseDto(createResult.Value, request.Email));
    }
}