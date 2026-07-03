using MediatR;
using SocialHub.Application.Common.Results;

namespace SocialHub.Application.Common.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
