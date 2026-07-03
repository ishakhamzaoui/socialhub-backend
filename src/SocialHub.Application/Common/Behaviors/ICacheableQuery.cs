namespace SocialHub.Application.Common.Behaviors;
 
/// <summary>
/// Implement on a query to opt it into the caching pipeline behavior.
/// </summary>
public interface ICacheableQuery
{
    string CacheKey { get; }
 
    TimeSpan? Expiration => null;
}