using MediatR;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Common.Messaging;
 
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
