namespace SocialHub.Application.Features.Posts;
 
/// <summary>Same shape as Users/Follow's PagedUserListDto — kept consistent across features.</summary>
public sealed record PagedPostListDto(IReadOnlyList<PostDto> Posts, int TotalCount, int Page, int PageSize);