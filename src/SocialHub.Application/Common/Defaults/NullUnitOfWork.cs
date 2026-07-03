using SocialHub.Application.Common.Interfaces;
 
namespace SocialHub.Application.Common.Defaults;
 
/// <summary>
/// Default IUnitOfWork until Phase 2 registers the EF-Core-backed
/// implementation. SaveChangesAsync is a no-op (nothing to persist yet).
/// </summary>
public sealed class NullUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
 
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
 
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
 
    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}