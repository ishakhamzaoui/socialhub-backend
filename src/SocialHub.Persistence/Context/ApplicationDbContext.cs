using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialHub.Application.Common.Events;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Domain.Comments;
using SocialHub.Domain.Common;
using SocialHub.Domain.Media;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
using SocialHub.Domain.Users;
using SocialHub.Identity.Models;
 
namespace SocialHub.Persistence.Context;
 
public class ApplicationDbContext
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>,
      IApplicationDbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;
 
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }
 
    public DbSet<Hashtag> Hashtags => Set<Hashtag>();
 
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
 
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
 
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
 
    public DbSet<Follow> Follows => Set<Follow>();
 
    public DbSet<UserBlock> UserBlocks => Set<UserBlock>();
 
    public DbSet<UserMute> UserMutes => Set<UserMute>();
 
    // Phase 6 — Posts.
    public DbSet<Post> Posts => Set<Post>();
 
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
 
    public DbSet<PostHashtag> PostHashtags => Set<PostHashtag>();
 
    public DbSet<PostMention> PostMentions => Set<PostMention>();
 
    public DbSet<PostRepost> PostReposts => Set<PostRepost>();
 
    // Phase 7 — Comments & Reactions.
    public DbSet<Comment> Comments => Set<Comment>();
 
    public DbSet<CommentMention> CommentMentions => Set<CommentMention>();
 
    public DbSet<CommentReaction> CommentReactions => Set<CommentReaction>();
 
    public DbSet<CommentReport> CommentReports => Set<CommentReport>();
 
    // Phase 8 — Feed Engine. PostReaction added (script 51) to give Posts
    // the like/reaction capability they previously lacked entirely.
    public DbSet<PostReaction> PostReactions => Set<PostReaction>();
 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder); // Identity's own entity configuration (AspNetUsers, etc.)
    }
 
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Snapshot entities carrying domain events *before* SaveChanges, since
        // ClearDomainEvents() happens as part of dispatch, not tracking.
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();
 
        var result = await base.SaveChangesAsync(cancellationToken);
 
        // Domain events are published only after the transaction has
        // committed successfully — SocialHub-Backend-Specification.md §16.
        await _domainEventDispatcher.DispatchAndClearEvents(entitiesWithEvents, cancellationToken);
 
        return result;
    }
}