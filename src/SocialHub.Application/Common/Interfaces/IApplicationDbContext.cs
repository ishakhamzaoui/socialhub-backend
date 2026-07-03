using Microsoft.EntityFrameworkCore;
 
namespace SocialHub.Application.Common.Interfaces;
 
/// <summary>
/// Persistence-agnostic view of the EF Core DbContext, so the Application
/// layer can depend on an abstraction rather than SocialHub.Persistence.
/// Implemented by ApplicationDbContext in Phase 2.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
 
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}