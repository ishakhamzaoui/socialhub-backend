using System.Threading.RateLimiting;
 
namespace SocialHub.Infrastructure.RateLimiting;
 
internal sealed class RedisRateLimitLease : RateLimitLease
{
    private readonly TimeSpan? _retryAfter;
 
    public RedisRateLimitLease(bool isAcquired, TimeSpan? retryAfter)
    {
        IsAcquired = isAcquired;
        _retryAfter = retryAfter;
    }
 
    public override bool IsAcquired { get; }
 
    public override IEnumerable<string> MetadataNames =>
        _retryAfter.HasValue ? new[] { MetadataName.RetryAfter.Name } : Array.Empty<string>();
 
    public override bool TryGetMetadata(string metadataName, out object? metadata)
    {
        if (metadataName == MetadataName.RetryAfter.Name && _retryAfter.HasValue)
        {
            metadata = _retryAfter.Value;
            return true;
        }
 
        metadata = null;
        return false;
    }
}