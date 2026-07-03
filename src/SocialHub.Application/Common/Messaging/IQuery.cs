using MediatR;
using SocialHub.Application.Common.Results;

namespace SocialHub.Application.Common.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
