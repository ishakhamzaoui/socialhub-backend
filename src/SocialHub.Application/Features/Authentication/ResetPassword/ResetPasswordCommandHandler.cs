using MediatR;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Authentication.ResetPassword;
 
public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
 
    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
 
    public Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken) =>
        _identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);
}