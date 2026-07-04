using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Messaging;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Authentication.Register;
 
public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;
    private readonly IAppUrlProvider _appUrlProvider;
 
    public RegisterCommandHandler(IIdentityService identityService, IEmailSender emailSender, IAppUrlProvider appUrlProvider)
    {
        _identityService = identityService;
        _emailSender = emailSender;
        _appUrlProvider = appUrlProvider;
    }
 
    public async Task<Result<RegisterResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var createResult = await _identityService.CreateUserAsync(request.Email, request.Password, cancellationToken);
        if (createResult.IsFailure)
        {
            return Result.Failure<RegisterResponseDto>(createResult.Error);
        }
 
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