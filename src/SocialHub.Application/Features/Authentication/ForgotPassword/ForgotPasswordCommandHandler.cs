using MediatR;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Authentication.ForgotPassword;
 
public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;
    private readonly IAppUrlProvider _appUrlProvider;
 
    public ForgotPasswordCommandHandler(IIdentityService identityService, IEmailSender emailSender, IAppUrlProvider appUrlProvider)
    {
        _identityService = identityService;
        _emailSender = emailSender;
        _appUrlProvider = appUrlProvider;
    }
 
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenResult = await _identityService.GeneratePasswordResetTokenAsync(request.Email, cancellationToken);
 
        // Always succeed from the caller's point of view — never reveal
        // whether an email address is registered (prevents enumeration).
        if (tokenResult.IsSuccess && tokenResult.Value is not null)
        {
            var resetUrl = _appUrlProvider.BuildPasswordResetUrl(request.Email, tokenResult.Value);
            var body = $"""
                <p>We received a request to reset your SocialHub password.</p>
                <p><a href="{resetUrl}">{resetUrl}</a></p>
                <p>(Dev note: the raw token is <code>{tokenResult.Value}</code> if you need to call
                POST /api/v1/auth/reset-password directly, e.g. via Swagger, before a frontend exists.)</p>
                <p>If you didn't request this, you can safely ignore this email.</p>
                """;
 
            await _emailSender.SendAsync(request.Email, "Reset your SocialHub password", body, cancellationToken);
        }
 
        return Result.Success();
    }
}