using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Posts;
using SocialHub.Domain.Media;
using SocialHub.Domain.Posts;
using SocialHub.Domain.Shared;
using SocialHub.Domain.Users;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Posts;
 
public class CreatePostCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly IMediaAssetRepository _mediaAssetRepository = Substitute.For<IMediaAssetRepository>();
    private readonly IHashtagRepository _hashtagRepository = Substitute.For<IHashtagRepository>();
    private readonly IUserProfileRepository _userProfileRepository = Substitute.For<IUserProfileRepository>();
    private readonly IUserBlockRepository _userBlockRepository = Substitute.For<IUserBlockRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private CreatePostCommandHandler CreateHandler() =>
        new(_currentUserService, _postRepository, _mediaAssetRepository, _hashtagRepository, _userProfileRepository, _userBlockRepository, _unitOfWork);
 
    private static CreatePostCommand SimpleCommand(string? content = "hello world") =>
        new(content, PostVisibility.Public, PostType.Original, null, PostStatus.Published, null, null, null, null);
 
    [Fact]
    public async Task Handle_Should_Succeed_For_SimplePublishedPost()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
 
        var result = await CreateHandler().Handle(SimpleCommand(), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(PostStatus.Published);
        await _postRepository.Received(1).AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_QuoteTarget_NotFound()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var originalPostId = Guid.NewGuid();
        _postRepository.GetByIdAsync(originalPostId, Arg.Any<CancellationToken>()).Returns((Post?)null);
 
        var command = new CreatePostCommand("quoting", PostVisibility.Public, PostType.Quote, originalPostId, PostStatus.Published, null, null, null, null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_QuoteTarget_Blocked()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var original = Post.CreatePublished(Guid.NewGuid(), "original", PostVisibility.Public);
        _postRepository.GetByIdAsync(original.Id, Arg.Any<CancellationToken>()).Returns(original);
        _userBlockRepository.IsBlockedEitherDirectionAsync(authorId, original.AuthorId, Arg.Any<CancellationToken>()).Returns(true);
 
        var command = new CreatePostCommand("quoting", PostVisibility.Public, PostType.Quote, original.Id, PostStatus.Published, null, null, null, null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_MediaAsset_NotOwnedByAuthor()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var mediaId = Guid.NewGuid();
        _mediaAssetRepository.GetByIdForOwnerAsync(mediaId, authorId, Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);
 
        var command = new CreatePostCommand(null, PostVisibility.Public, PostType.Original, null, PostStatus.Published, null, new[] { mediaId }, null, null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Media.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_CreateNewHashtag_When_NotAlreadyExisting()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        _hashtagRepository.GetByNormalizedTagAsync("DOTNET", Arg.Any<CancellationToken>()).Returns((Hashtag?)null);
 
        var command = new CreatePostCommand("hello #dotnet", PostVisibility.Public, PostType.Original, null, PostStatus.Published, null, null, new[] { "dotnet" }, null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        await _hashtagRepository.Received(1).AddAsync(Arg.Is<Hashtag>(h => h.NormalizedTag == "DOTNET"), Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_SkipMention_When_MentionedUserHasNoProfile()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var mentionedUserId = Guid.NewGuid();
        _userProfileRepository.GetByUserIdAsync(mentionedUserId, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);
 
        var command = new CreatePostCommand("hello @ghost", PostVisibility.Public, PostType.Original, null, PostStatus.Published, null, null, null, new[] { mentionedUserId });
        var result = await CreateHandler().Handle(command, CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        result.Value.MentionedUserIds.Should().BeEmpty();
    }
}