namespace SocialHub.Application.Features.Comments;
 
public sealed record PagedCommentListDto(IReadOnlyList<CommentDto> Comments, int TotalCount, int Page, int PageSize);