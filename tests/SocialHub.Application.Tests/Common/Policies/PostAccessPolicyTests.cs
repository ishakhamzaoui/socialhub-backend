using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Policies;
using SocialHub.Domain.Posts;
using Xunit;
 
namespace SocialHub.Application.Tests.Common.Policies;
 
public class PostAccessPolicyTests
{
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();
    private readonly IUserBlockRepository _userBlockRepository = Substitute.For<IUserBlockRepository>();
 
    [Fact]
    public async Task Evaluate_Should_ReturnOwner_When_RequesterIsAuthor_RegardlessOfStatus()
    {
        var authorId = Guid.NewGuid();
        var post = Post.CreateDraft(authorId, "hello", PostVisibility.Private);
 
        var result = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, authorId);
 
        result.Should().Be(PostAccessResult.Owner);
    }
 
    [Fact]
    public async Task Evaluate_Should_ReturnBlocked_When_BlockedEitherDirection()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public);
        var requesterId = Guid.NewGuid();
        _userBlockRepository.IsBlockedEitherDirectionAsync(post.AuthorId, requesterId, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId);
 
        result.Should().Be(PostAccessResult.Blocked);
    }
 
    [Theory]
    [InlineData(PostStatus.Draft)]
    [InlineData(PostStatus.Scheduled)]
    [InlineData(PostStatus.Archived)]
    public async Task Evaluate_Should_ReturnDenied_When_NonOwner_And_NotPublished(PostStatus _)
    {
        var post = Post.CreateDraft(Guid.NewGuid(), "hello", PostVisibility.Public);
        var requesterId = Guid.NewGuid();
        _userBlockRepository.IsBlockedEitherDirectionAsync(post.AuthorId, requesterId, Arg.Any<CancellationToken>()).Returns(false);
 
        var result = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId);
 
        result.Should().Be(PostAccessResult.Denied);
    }
 
    [Fact]
    public async Task Evaluate_Should_ReturnAllowed_When_Published_And_Public()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Public);
        var requesterId = Guid.NewGuid();
        _userBlockRepository.IsBlockedEitherDirectionAsync(post.AuthorId, requesterId, Arg.Any<CancellationToken>()).Returns(false);
 
        var result = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId);
 
        result.Should().Be(PostAccessResult.Allowed);
    }
 
    [Fact]
    public async Task Evaluate_Should_ReturnAllowed_When_Published_And_Unlisted()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Unlisted);
        var requesterId = Guid.NewGuid();
        _userBlockRepository.IsBlockedEitherDirectionAsync(post.AuthorId, requesterId, Arg.Any<CancellationToken>()).Returns(false);
 
        var result = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId);
 
        result.Should().Be(PostAccessResult.Allowed);
    }
 
    [Fact]
    public async Task Evaluate_Should_ReturnDenied_When_Private()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.Private);
        var requesterId = Guid.NewGuid();
        _userBlockRepository.IsBlockedEitherDirectionAsync(post.AuthorId, requesterId, Arg.Any<CancellationToken>()).Returns(false);
 
        var result = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId);
 
        result.Should().Be(PostAccessResult.Denied);
    }
 
    [Fact]
    public async Task Evaluate_Should_ReturnAllowed_When_FollowersOnly_And_RequesterFollowsAuthor()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.FollowersOnly);
        var requesterId = Guid.NewGuid();
        _userBlockRepository.IsBlockedEitherDirectionAsync(post.AuthorId, requesterId, Arg.Any<CancellationToken>()).Returns(false);
        _followRepository.ExistsAsync(requesterId, post.AuthorId, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId);
 
        result.Should().Be(PostAccessResult.Allowed);
    }
 
    [Fact]
    public async Task Evaluate_Should_ReturnDenied_When_FollowersOnly_And_RequesterDoesNotFollowAuthor()
    {
        var post = Post.CreatePublished(Guid.NewGuid(), "hello", PostVisibility.FollowersOnly);
        var requesterId = Guid.NewGuid();
        _userBlockRepository.IsBlockedEitherDirectionAsync(post.AuthorId, requesterId, Arg.Any<CancellationToken>()).Returns(false);
        _followRepository.ExistsAsync(requesterId, post.AuthorId, Arg.Any<CancellationToken>()).Returns(false);
 
        var result = await PostAccessPolicy.EvaluateAsync(_followRepository, _userBlockRepository, post, requesterId);
 
        result.Should().Be(PostAccessResult.Denied);
    }
}