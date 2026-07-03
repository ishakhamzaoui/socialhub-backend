namespace SocialHub.Domain.Common;

/// <summary>
/// Marks an entity as the root of an aggregate boundary. Repositories are
/// expected to operate on aggregate roots only.
/// </summary>
public interface IAggregateRoot
{
}