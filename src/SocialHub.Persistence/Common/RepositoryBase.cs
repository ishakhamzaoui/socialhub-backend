using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Specifications;
using SocialHub.Domain.Common;
 
namespace SocialHub.Persistence.Common;
 
public class RepositoryBase<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : BaseEntity
{
    private readonly IApplicationDbContext _context;
 
    public RepositoryBase(IApplicationDbContext context)
    {
        _context = context;
    }
 
    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default) =>
        await _context.Set<TEntity>().FindAsync(new object?[] { id }, cancellationToken);
 
    public async Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default) =>
        await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), specification).ToListAsync(cancellationToken);
 
    public async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default) =>
        await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), specification).FirstOrDefaultAsync(cancellationToken);
 
    public async Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default) =>
        await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), specification).CountAsync(cancellationToken);
 
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
 
    public void Update(TEntity entity) => _context.Set<TEntity>().Update(entity);
 
    public void Remove(TEntity entity) => _context.Set<TEntity>().Remove(entity);
}