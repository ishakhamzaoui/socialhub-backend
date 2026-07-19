using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Comments;
using SocialHub.Persistence.Common;
 
namespace SocialHub.Persistence.Repositories;
 
public sealed class CommentReportRepository : RepositoryBase<CommentReport, Guid>, ICommentReportRepository
{
    private readonly IApplicationDbContext _context;
 
    public CommentReportRepository(IApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }
 
    public async Task<bool> ExistsAsync(Guid commentId, Guid reporterId, CancellationToken cancellationToken = default) =>
        await _context.Set<CommentReport>()
            .AnyAsync(r => r.CommentId == commentId && r.ReporterId == reporterId, cancellationToken);
}