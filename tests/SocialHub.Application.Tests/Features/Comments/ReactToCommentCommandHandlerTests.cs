using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Comments;
using SocialHub.Domain.Comments;
using SocialHub.Domain.Shared;
using SocialHub.Domain.Posts;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Comments;
 
public class ReactToCommentCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly ICommentReactionRepository _commentReactionRepository = Substitute.For<ICommentReactionRepository>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();
    private readonly IUserBlockRepository _userBlockRepository = Substitute.For<IUserBlockRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private ReactToCommentCommandHandler CreateHandler() =>
        new(_currentUserService, _commentRepository, _commentReactionRepository, _postRepository, _followRepository, _userBlockRepository, _unitOfWork);
 
    private (Post Post, Comment Comment) PublishedPostWithComment()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "post", PostVisibility.Public);
        var comment = Comment.Create(post.Id, Guid.NewGuid(), "a comment");
        return (post, comment);
    }
 
    [Fact]
    public async Task Handle_Should_AddNewReaction_When_NoneExists()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var (post, comment) = PublishedPostWithComment();
        _commentRepository.GetByIdWithDetailsAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        _commentReactionRepository.GetAsync(comment.Id, requesterId, Arg.Any<CancellationToken>()).Returns((CommentReaction?)null);
 
        var result = await CreateHandler().Handle(new ReactToCommentCommand(comment.Id, ReactionType.Love), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        await _commentReactionRepository.Received(1).AddAsync(Arg.Is<CommentReaction>(r => r.Type == ReactionType.Love), Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_ChangeType_When_ReactionAlreadyExists()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var (post, comment) = PublishedPostWithComment();
        _commentRepository.GetByIdWithDetailsAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        var existing = CommentReaction.Create(comment.Id, requesterId, ReactionType.Like);
        _commentReactionRepository.GetAsync(comment.Id, requesterId, Arg.Any<CancellationToken>()).Returns(existing);
 
        var result = await CreateHandler().Handle(new ReactToCommentCommand(comment.Id, ReactionType.Sad), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        existing.Type.Should().Be(ReactionType.Sad);
        await _commentReactionRepository.DidNotReceive().AddAsync(Arg.Any<CommentReaction>(), Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_CommentAuthorAndRequesterAreBlocked()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var (post, comment) = PublishedPostWithComment();
        _commentRepository.GetByIdWithDetailsAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        // Post-level block check passes (post author != comment author here),
        // but the single-comment-level check (confirmed decision #5) between
        // requester and the COMMENT's own author should still block this.
        _userBlockRepository.IsBlockedEitherDirectionAsync(comment.AuthorId, requesterId, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await CreateHandler().Handle(new ReactToCommentCommand(comment.Id, ReactionType.Like), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_CommentIsDeleted()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var (_, comment) = PublishedPostWithComment();
        comment.MarkDeleted();
        _commentRepository.GetByIdWithDetailsAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);
 
        var result = await CreateHandler().Handle(new ReactToCommentCommand(comment.Id, ReactionType.Like), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.NotFound");
    }
}