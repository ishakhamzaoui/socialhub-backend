using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Comments;
using SocialHub.Domain.Comments;
using SocialHub.Domain.Posts;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Comments;
 
public class PinCommentCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private PinCommentCommandHandler CreateHandler() => new(_currentUserService, _commentRepository, _postRepository, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_Fail_When_RequesterIsNotPostAuthor()
    {
        var postAuthorId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var post = Post.CreatePublished(postAuthorId, "post", PostVisibility.Public);
        var comment = Comment.Create(post.Id, Guid.NewGuid(), "a comment");
        _commentRepository.GetByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
 
        var result = await CreateHandler().Handle(new PinCommentCommand(comment.Id), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.PinForbidden");
        comment.IsPinned.Should().BeFalse();
    }
 
    [Fact]
    public async Task Handle_Should_UnpinPreviouslyPinnedComment_And_PinTheNewOne()
    {
        var postAuthorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(postAuthorId.ToString());
        var post = Post.CreatePublished(postAuthorId, "post", PostVisibility.Public);
        var previouslyPinned = Comment.Create(post.Id, Guid.NewGuid(), "old pin");
        previouslyPinned.Pin();
        var toPin = Comment.Create(post.Id, Guid.NewGuid(), "new pin");
 
        _commentRepository.GetByIdAsync(toPin.Id, Arg.Any<CancellationToken>()).Returns(toPin);
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        _commentRepository.GetPinnedCommentForPostAsync(post.Id, Arg.Any<CancellationToken>()).Returns(previouslyPinned);
 
        var result = await CreateHandler().Handle(new PinCommentCommand(toPin.Id), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        previouslyPinned.IsPinned.Should().BeFalse();
        toPin.IsPinned.Should().BeTrue();
    }
}