using FluentAssertions;
using SocialHub.Domain.Comments;
using SocialHub.Persistence.Repositories;
using Xunit;
 
namespace SocialHub.Infrastructure.Tests.Persistence;
 
[Collection("Postgres collection")]
public class CommentRepositoryTests
{
    private readonly PostgresDatabaseFixture _fixture;
 
    public CommentRepositoryTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
 
    [Fact]
    public async Task GetByIdWithDetailsAsync_Should_LoadMentions_ThroughFreshContext()
    {
        await using var context = _fixture.CreateContext();
        var repository = new CommentRepository(context);
 
        var mentionedUserId = Guid.NewGuid();
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "hello @someone");
        comment.AddMention(mentionedUserId);
        await repository.AddAsync(comment);
        await context.SaveChangesAsync();
 
        // Fresh context — the only way to genuinely confirm the Mentions
        // owned-collection backing-field EF configuration round-trips
        // correctly, same requirement Phase 6 established for Post/PostMedia.
        await using var freshContext = _fixture.CreateContext();
        var freshRepository = new CommentRepository(freshContext);
        var reloaded = await freshRepository.GetByIdWithDetailsAsync(comment.Id);
 
        reloaded.Should().NotBeNull();
        reloaded!.Mentions.Should().ContainSingle(m => m.MentionedUserId == mentionedUserId);
    }
 
    [Fact]
    public async Task GetByIdForAuthorAsync_Should_ReturnNull_When_DifferentAuthor()
    {
        await using var context = _fixture.CreateContext();
        var repository = new CommentRepository(context);
 
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "hello");
        await repository.AddAsync(comment);
        await context.SaveChangesAsync();
 
        var result = await repository.GetByIdForAuthorAsync(comment.Id, Guid.NewGuid());
 
        result.Should().BeNull();
    }
 
    [Fact]
    public async Task GetTopLevelForPostAsync_Should_OrderPinnedFirst_ThenChronological()
    {
        await using var context = _fixture.CreateContext();
        var repository = new CommentRepository(context);
 
        var postId = Guid.NewGuid();
        var first = Comment.Create(postId, Guid.NewGuid(), "first");
        await repository.AddAsync(first);
        await context.SaveChangesAsync();
 
        var second = Comment.Create(postId, Guid.NewGuid(), "second, then pinned");
        second.Pin();
        await repository.AddAsync(second);
        await context.SaveChangesAsync();
 
        var (comments, total) = await repository.GetTopLevelForPostAsync(postId, page: 1, pageSize: 20);
 
        total.Should().Be(2);
        comments.Should().HaveCount(2);
        comments[0].Id.Should().Be(second.Id); // pinned sorts first regardless of creation order
        comments[1].Id.Should().Be(first.Id);
    }
 
    [Fact]
    public async Task GetTopLevelForPostAsync_Should_ExcludeAuthorIds_And_KeepTotalAccurate()
    {
        await using var context = _fixture.CreateContext();
        var repository = new CommentRepository(context);
 
        var postId = Guid.NewGuid();
        var blockedAuthorId = Guid.NewGuid();
        var visible = Comment.Create(postId, Guid.NewGuid(), "visible");
        var fromBlockedAuthor = Comment.Create(postId, blockedAuthorId, "should be excluded");
        await repository.AddAsync(visible);
        await repository.AddAsync(fromBlockedAuthor);
        await context.SaveChangesAsync();
 
        var (comments, total) = await repository.GetTopLevelForPostAsync(postId, page: 1, pageSize: 20, excludeAuthorIds: new[] { blockedAuthorId });
 
        // Total reflects the FILTERED count, not the unfiltered one — this
        // is exactly what script 45's correction exists to guarantee.
        total.Should().Be(1);
        comments.Should().ContainSingle(c => c.Id == visible.Id);
    }
 
    [Fact]
    public async Task GetRepliesAsync_Should_ReturnOnlyDirectChildren_Of_TheGivenParent()
    {
        await using var context = _fixture.CreateContext();
        var repository = new CommentRepository(context);
 
        var postId = Guid.NewGuid();
        var parent = Comment.Create(postId, Guid.NewGuid(), "parent");
        await repository.AddAsync(parent);
        await context.SaveChangesAsync();
 
        var reply = Comment.Create(postId, Guid.NewGuid(), "a reply", parent.Id);
        var unrelated = Comment.Create(postId, Guid.NewGuid(), "top-level, not a reply");
        await repository.AddAsync(reply);
        await repository.AddAsync(unrelated);
        await context.SaveChangesAsync();
 
        var (replies, total) = await repository.GetRepliesAsync(parent.Id, page: 1, pageSize: 20);
 
        total.Should().Be(1);
        replies.Should().ContainSingle(r => r.Id == reply.Id);
    }
 
    [Fact]
    public async Task GetPinnedCommentForPostAsync_Should_ReturnThePinnedComment()
    {
        await using var context = _fixture.CreateContext();
        var repository = new CommentRepository(context);
 
        var postId = Guid.NewGuid();
        var pinned = Comment.Create(postId, Guid.NewGuid(), "pinned");
        pinned.Pin();
        var notPinned = Comment.Create(postId, Guid.NewGuid(), "not pinned");
        await repository.AddAsync(pinned);
        await repository.AddAsync(notPinned);
        await context.SaveChangesAsync();
 
        var result = await repository.GetPinnedCommentForPostAsync(postId);
 
        result.Should().NotBeNull();
        result!.Id.Should().Be(pinned.Id);
    }
 
    [Fact]
    public async Task GetReplyCountAsync_Should_CountOnlyDirectReplies()
    {
        await using var context = _fixture.CreateContext();
        var repository = new CommentRepository(context);
 
        var postId = Guid.NewGuid();
        var parent = Comment.Create(postId, Guid.NewGuid(), "parent");
        await repository.AddAsync(parent);
        await context.SaveChangesAsync();
 
        var reply1 = Comment.Create(postId, Guid.NewGuid(), "reply 1", parent.Id);
        var reply2 = Comment.Create(postId, Guid.NewGuid(), "reply 2", parent.Id);
        await repository.AddAsync(reply1);
        await repository.AddAsync(reply2);
        await context.SaveChangesAsync();
 
        var count = await repository.GetReplyCountAsync(parent.Id);
 
        count.Should().Be(2);
    }
}