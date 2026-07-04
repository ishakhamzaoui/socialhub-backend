using MediatR;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Authentication.ConfirmEmail;
 
public sealed class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
 
    public ConfirmEmailCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
 
    public Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken) =>
        _identityService.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
}