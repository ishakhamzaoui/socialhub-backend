using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Comments;
using SocialHub.Domain.Comments;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Comments;
 
public class DeleteCommentCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ICommentRepository _commentRepository = Substitute.For<ICommentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private DeleteCommentCommandHandler CreateHandler() => new(_currentUserService, _commentRepository, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_SoftDelete_And_NeverCallRemove()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var comment = Comment.Create(Guid.NewGuid(), authorId, "hello");
        _commentRepository.GetByIdForAuthorAsync(comment.Id, authorId, Arg.Any<CancellationToken>()).Returns(comment);
 
        var result = await CreateHandler().Handle(new DeleteCommentCommand(comment.Id), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        comment.IsDeleted.Should().BeTrue();
        comment.Content.Should().BeNull();
        // Unlike DeletePostCommandHandler, this is a soft delete — the row
        // must never be removed from the DbSet.
        _commentRepository.DidNotReceive().Remove(Arg.Any<Comment>());
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_NotFoundOrNotOwned()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        _commentRepository.GetByIdForAuthorAsync(Arg.Any<Guid>(), authorId, Arg.Any<CancellationToken>()).Returns((Comment?)null);
 
        var result = await CreateHandler().Handle(new DeleteCommentCommand(Guid.NewGuid()), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_Fail_When_AlreadyDeleted()
    {
        var authorId = Guid.NewGuid();
        _currentUserService.UserId.Returns(authorId.ToString());
        var comment = Comment.Create(Guid.NewGuid(), authorId, "hello");
        comment.MarkDeleted();
        _commentRepository.GetByIdForAuthorAsync(comment.Id, authorId, Arg.Any<CancellationToken>()).Returns(comment);
 
        var result = await CreateHandler().Handle(new DeleteCommentCommand(comment.Id), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.Deleted");
    }
}