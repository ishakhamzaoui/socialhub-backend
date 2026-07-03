using SocialHub.Application.Common.Specifications;
using SocialHub.Domain.Common;
 
namespace SocialHub.Application.Common.Interfaces;
 
public interface IRepository<TEntity, in TId> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
 
    Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
 
    Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
 
    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
 
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
 
    void Update(TEntity entity);
 
    void Remove(TEntity entity);
}