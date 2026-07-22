using System.Text;
 
namespace SocialHub.Application.Common.Pagination;
 
/// <summary>
/// Opaque Base64 cursor over a (SortKey, TieBreakerId) pair — confirmed
/// design (Phase 8 kickoff). Deliberately generic about what SortKey means:
/// for time-ordered feeds (following/chronological/personalized) it's
/// CreatedAtUtc.Ticks; for the trending feed it's a repost-count-based
/// score (see IFeedRepository.GetTrendingFeedAsync's remarks). Only the
/// query handler that creates/consumes a given feed type's cursor needs to
/// know which interpretation applies — FeedCursor itself just encodes and
/// decodes the pair.
/// </summary>
public readonly record struct FeedCursor(long SortKey, Guid TieBreakerId)
{
    public string Encode()
    {
        var raw = $"{SortKey}:{TieBreakerId:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }
 
    public static bool TryDecode(string? encoded, out FeedCursor cursor)
    {
        cursor = default;
 
        if (string.IsNullOrWhiteSpace(encoded))
        {
            return false;
        }
 
        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var separatorIndex = raw.IndexOf(':');
            if (separatorIndex < 0)
            {
                return false;
            }
 
            if (!long.TryParse(raw[..separatorIndex], out var sortKey))
            {
                return false;
            }
 
            if (!Guid.TryParse(raw[(separatorIndex + 1)..], out var tieBreakerId))
            {
                return false;
            }
 
            cursor = new FeedCursor(sortKey, tieBreakerId);
            return true;
        }
        catch (FormatException)
        {
            // A malformed/tampered cursor degrades to "start from the
            // beginning" rather than a 500 — same defensive spirit as
            // RedisCacheService's JsonException handling.
            return false;
        }
    }
}