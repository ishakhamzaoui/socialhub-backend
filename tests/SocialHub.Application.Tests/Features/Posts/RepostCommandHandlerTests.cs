using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Posts;
using SocialHub.Domain.Posts;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Posts;
 
public class RepostCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly IPostRepostRepository _postRepostRepository = Substitute.For<IPostRepostRepository>();
    private readonly IUserBlockRepository _userBlockRepository = Substitute.For<IUserBlockRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private RepostCommandHandler CreateHandler() =>
        new(_currentUserService, _postRepository, _postRepostRepository, _userBlockRepository, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_Fail_When_OriginalNotPublished()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var original = Post.CreateDraft(Guid.NewGuid(), "draft", PostVisibility.Public);
        _postRepository.GetByIdAsync(original.Id, Arg.Any<CancellationToken>()).Returns(original);
 
        var result = await CreateHandler().Handle(new RepostCommand(original.Id), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_BlockedEitherDirection()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var original = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public);
        _postRepository.GetByIdAsync(original.Id, Arg.Any<CancellationToken>()).Returns(original);
        _userBlockRepository.IsBlockedEitherDirectionAsync(requesterId, original.AuthorId, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await CreateHandler().Handle(new RepostCommand(original.Id), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_AlreadyReposted()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var original = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public);
        _postRepository.GetByIdAsync(original.Id, Arg.Any<CancellationToken>()).Returns(original);
        _userBlockRepository.IsBlockedEitherDirectionAsync(requesterId, original.AuthorId, Arg.Any<CancellationToken>()).Returns(false);
        _postRepostRepository.ExistsAsync(requesterId, original.Id, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await CreateHandler().Handle(new RepostCommand(original.Id), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Repost.AlreadyExists");
    }
 
    [Fact]
    public async Task Handle_Should_Succeed_When_Valid()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var original = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public);
        _postRepository.GetByIdAsync(original.Id, Arg.Any<CancellationToken>()).Returns(original);
        _userBlockRepository.IsBlockedEitherDirectionAsync(requesterId, original.AuthorId, Arg.Any<CancellationToken>()).Returns(false);
        _postRepostRepository.ExistsAsync(requesterId, original.Id, Arg.Any<CancellationToken>()).Returns(false);
 
        var result = await CreateHandler().Handle(new RepostCommand(original.Id), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        await _postRepostRepository.Received(1).AddAsync(
            Arg.Is<PostRepost>(r => r.UserId == requesterId && r.OriginalPostId == original.Id),
            Arg.Any<CancellationToken>());
    }
}