using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Comments;
using SocialHub.Domain.Comments;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Comments;
 
public class CreateCommentCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly ICommentReactionRepository _commentReactionRepository = Substitute.For<ICommentReactionRepository>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();
    private readonly IUserBlockRepository _userBlockRepository = Substitute.For<IUserBlockRepository>();
    private readonly IUserProfileRepository _userProfileRepository = Substitute.For<IUserProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private CreateCommentCommandHandler CreateHandler() =>
        new(_currentUserService, _commentRepository, _commentReactionRepository, _postRepository, _followRepository, _userBlockRepository, _userProfileRepository, _unitOfWork);
 
    private Post PublishedPost(Guid? authorId = null) =>
        Post.CreatePublished(authorId ?? Guid.NewGuid(), "post content", PostVisibility.Public);
 
    [Fact]
    public async Task Handle_Should_Succeed_For_TopLevelComment_On_PublicPost()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var post = PublishedPost();
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
 
        var command = new CreateCommentCommand(post.Id, null, "nice post!", null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("nice post!");
        result.Value.ParentCommentId.Should().BeNull();
        await _commentRepository.Received(1).AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_PostNotFound()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var postId = Guid.NewGuid();
        _postRepository.GetByIdAsync(postId, Arg.Any<CancellationToken>()).Returns((Post?)null);
 
        var command = new CreateCommentCommand(postId, null, "hello", null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_Blocked_NotForbidden()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var post = PublishedPost();
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        _userBlockRepository.IsBlockedEitherDirectionAsync(post.AuthorId, requesterId, Arg.Any<CancellationToken>()).Returns(true);
 
        var command = new CreateCommentCommand(post.Id, null, "hello", null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        // Confirms the non-leaking convention: a block never surfaces as
        // Forbidden, only ever NotFound.
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnForbidden_When_PrivatePost_And_NotFollowing()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var post = Post.CreatePublished(Guid.NewGuid(), "private post", PostVisibility.Private);
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        _userBlockRepository.IsBlockedEitherDirectionAsync(post.AuthorId, requesterId, Arg.Any<CancellationToken>()).Returns(false);
 
        var command = new CreateCommentCommand(post.Id, null, "hello", null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.Private");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_PostNotPublished_EvenForOwner()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var draft = Post.CreateDraft(authorId, "draft post", PostVisibility.Public);
        _postRepository.GetByIdAsync(draft.Id, Arg.Any<CancellationToken>()).Returns(draft);
 
        var command = new CreateCommentCommand(draft.Id, null, "hello", null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotPublished");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_ParentComment_BelongsToDifferentPost()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var post = PublishedPost();
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        var parent = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "on a different post");
        _commentRepository.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);
 
        var command = new CreateCommentCommand(post.Id, parent.Id, "a reply", null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_SkipMention_When_MentionedUserHasNoProfile()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var post = PublishedPost();
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        var mentionedUserId = Guid.NewGuid();
        _userProfileRepository.GetByUserIdAsync(mentionedUserId, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);
 
        var command = new CreateCommentCommand(post.Id, null, "hello @ghost", new[] { mentionedUserId });
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.MentionedUserIds.Should().BeEmpty();
    }
}