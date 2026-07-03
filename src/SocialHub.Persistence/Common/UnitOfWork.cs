using SocialHub.Application.Common.Interfaces;
 
namespace SocialHub.Persistence.Common;
 
/// <summary>
/// Minimal EF-Core-backed Unit of Work. BeginTransactionAsync/RollbackAsync
/// become real database transactions once ApplicationDbContext exists
/// (Phase 2, step 2.2) and exposes its DatabaseFacade for explicit
/// transaction control.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly IApplicationDbContext _context;
 
    public UnitOfWork(IApplicationDbContext context)
    {
        _context = context;
    }
 
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
 
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask; // TODO(Phase 2)
 
    public Task CommitAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);
 
    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask; // TODO(Phase 2)
}