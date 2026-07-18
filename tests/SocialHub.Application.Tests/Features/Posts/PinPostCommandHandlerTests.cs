using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Posts;
using SocialHub.Domain.Posts;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Posts;
 
public class PinPostCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private PinPostCommandHandler CreateHandler() => new(_currentUserService, _postRepository, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_Fail_When_PostNotPublished()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var post = Post.CreateDraft(authorId, "hello", PostVisibility.Public);
        _postRepository.GetByIdForAuthorAsync(post.Id, authorId, Arg.Any<CancellationToken>()).Returns(post);
 
        var result = await CreateHandler().Handle(new PinPostCommand(post.Id), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotPublished");
    }
 
    [Fact]
    public async Task Handle_Should_UnpinPreviousPost_Before_PinningNewOne()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
 
        var previouslyPinned = Post.CreatePublished(authorId, "old", PostVisibility.Public);
        previouslyPinned.Pin();
        var newPost = Post.CreatePublished(authorId, "new", PostVisibility.Public);
 
        _postRepository.GetByIdForAuthorAsync(newPost.Id, authorId, Arg.Any<CancellationToken>()).Returns(newPost);
        _postRepository.GetPinnedPostAsync(authorId, Arg.Any<CancellationToken>()).Returns(previouslyPinned);
 
        var result = await CreateHandler().Handle(new PinPostCommand(newPost.Id), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        previouslyPinned.IsPinned.Should().BeFalse();
        newPost.IsPinned.Should().BeTrue();
    }
}