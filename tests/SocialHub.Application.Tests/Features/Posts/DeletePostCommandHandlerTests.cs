using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Posts;
using SocialHub.Domain.Posts;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Posts;
 
public class DeletePostCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private DeletePostCommandHandler CreateHandler() => new(_currentUserService, _postRepository, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_Fail_When_NotOwnedByRequester()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var postId = Guid.NewGuid();
        _postRepository.GetByIdForAuthorAsync(postId, authorId, Arg.Any<CancellationToken>()).Returns((Post?)null);
 
        var result = await CreateHandler().Handle(new DeletePostCommand(postId), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_RemovePost_And_Succeed()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var post = Post.CreatePublished(authorId, "hello", PostVisibility.Public);
        _postRepository.GetByIdForAuthorAsync(post.Id, authorId, Arg.Any<CancellationToken>()).Returns(post);
 
        var result = await CreateHandler().Handle(new DeletePostCommand(post.Id), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        _postRepository.Received(1).Remove(post);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}