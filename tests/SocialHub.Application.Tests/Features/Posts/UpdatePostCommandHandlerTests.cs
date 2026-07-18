using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Posts;
using SocialHub.Domain.Posts;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Posts;
 
public class UpdatePostCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly IMediaAssetRepository _mediaAssetRepository = Substitute.For<IMediaAssetRepository>();
    private readonly IHashtagRepository _hashtagRepository = Substitute.For<IHashtagRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private UpdatePostCommandHandler CreateHandler() =>
        new(_currentUserService, _postRepository, _mediaAssetRepository, _hashtagRepository, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_Fail_When_PostNotFound()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var postId = Guid.NewGuid();
        _postRepository.GetByIdForAuthorAsync(postId, authorId, Arg.Any<CancellationToken>()).Returns((Post?)null);
 
        var result = await CreateHandler().Handle(new UpdatePostCommand(postId, "new", PostVisibility.Public), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_PostIsArchived()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var post = Post.CreatePublished(authorId, "original", PostVisibility.Public);
        post.Archive();
        _postRepository.GetByIdForAuthorAsync(post.Id, authorId, Arg.Any<CancellationToken>()).Returns(post);
 
        var result = await CreateHandler().Handle(new UpdatePostCommand(post.Id, "new", PostVisibility.Public), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.Archived");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_ClearingContentWithNoMedia()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var post = Post.CreatePublished(authorId, "original", PostVisibility.Public);
        _postRepository.GetByIdForAuthorAsync(post.Id, authorId, Arg.Any<CancellationToken>()).Returns(post);
 
        var result = await CreateHandler().Handle(new UpdatePostCommand(post.Id, "", PostVisibility.Public), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.Invalid");
    }
 
    [Fact]
    public async Task Handle_Should_Succeed_And_UpdateContentAndVisibility()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var post = Post.CreatePublished(authorId, "original", PostVisibility.Public);
        _postRepository.GetByIdForAuthorAsync(post.Id, authorId, Arg.Any<CancellationToken>()).Returns(post);
 
        var result = await CreateHandler().Handle(new UpdatePostCommand(post.Id, "updated", PostVisibility.FollowersOnly), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("updated");
        result.Value.Visibility.Should().Be(PostVisibility.FollowersOnly);
    }
}