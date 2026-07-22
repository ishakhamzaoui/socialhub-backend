using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Posts;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Posts;
 
public class ReactToPostCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly IPostReactionRepository _postReactionRepository = Substitute.For<IPostReactionRepository>();
    private readonly IMediaAssetRepository _mediaAssetRepository = Substitute.For<IMediaAssetRepository>();
    private readonly IHashtagRepository _hashtagRepository = Substitute.For<IHashtagRepository>();
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly IFollowRepository _followRepository = Substitute.For<IFollowRepository>();
    private readonly IUserBlockRepository _userBlockRepository = Substitute.For<IUserBlockRepository>();
    private readonly IUserProfileRepository _userProfileRepository = Substitute.For<IUserProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private ReactToPostCommandHandler CreateHandler() => new(
        _currentUserService, _postRepository, _postReactionRepository, _mediaAssetRepository,
        _hashtagRepository, _commentRepository, _followRepository, _userBlockRepository,
        _userProfileRepository, _unitOfWork);
 
    private void SetUpDtoFactoryDependencies(Post post)
    {
        // PostDtoFactory.CreateAsync's own dependencies — none of this
        // test's assertions care about the resulting PostDto's shape, only
        // that the reaction itself was added/changed, so these are wired to
        // safe empty defaults.
        _commentRepository.GetTotalCommentCountAsync(post.Id, Arg.Any<CancellationToken>()).Returns(0);
        _postReactionRepository.GetCountsByTypeAsync(post.Id, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<ReactionType, int>());
    }
 
    [Fact]
    public async Task Handle_Should_AddNewReaction_When_NoneExists()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var post = Post.CreatePublished(Guid.NewGuid(), "a post", PostVisibility.Public);
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        _postReactionRepository.GetAsync(post.Id, requesterId, Arg.Any<CancellationToken>()).Returns((PostReaction?)null);
        SetUpDtoFactoryDependencies(post);
 
        var result = await CreateHandler().Handle(new ReactToPostCommand(post.Id, ReactionType.Love), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        await _postReactionRepository.Received(1).AddAsync(Arg.Is<PostReaction>(r => r.Type == ReactionType.Love), Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_ChangeType_When_ReactionAlreadyExists()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var post = Post.CreatePublished(Guid.NewGuid(), "a post", PostVisibility.Public);
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        var existing = PostReaction.Create(post.Id, requesterId, ReactionType.Like);
        _postReactionRepository.GetAsync(post.Id, requesterId, Arg.Any<CancellationToken>()).Returns(existing);
        SetUpDtoFactoryDependencies(post);
 
        var result = await CreateHandler().Handle(new ReactToPostCommand(post.Id, ReactionType.Sad), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        existing.Type.Should().Be(ReactionType.Sad);
        await _postReactionRepository.DidNotReceive().AddAsync(Arg.Any<PostReaction>(), Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_AuthorHasBlockedRequester()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var post = Post.CreatePublished(Guid.NewGuid(), "a post", PostVisibility.Public);
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
        // Same non-leaking convention as every other Blocked-path test in
        // this codebase — NotFound, never Forbidden.
        _userBlockRepository.IsBlockedEitherDirectionAsync(post.AuthorId, requesterId, Arg.Any<CancellationToken>()).Returns(true);
 
        var result = await CreateHandler().Handle(new ReactToPostCommand(post.Id, ReactionType.Like), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_PostIsNotPublished()
    {
        var requesterId = Guid.NewGuid();
        _currentUserService.UserId.Returns(requesterId.ToString());
        var post = Post.CreateDraft(Guid.NewGuid(), "a draft", PostVisibility.Public);
        _postRepository.GetByIdAsync(post.Id, Arg.Any<CancellationToken>()).Returns(post);
 
        var result = await CreateHandler().Handle(new ReactToPostCommand(post.Id, ReactionType.Like), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotPublished");
    }
}