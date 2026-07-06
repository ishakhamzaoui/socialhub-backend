using FluentAssertions;
using NSubstitute;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Features.Media;
using SocialHub.Domain.Media;
using Xunit;
 
namespace SocialHub.Application.Tests.Features.Media;
 
public class DeleteMediaCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IMediaAssetRepository _repository = Substitute.For<IMediaAssetRepository>();
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
 
    private DeleteMediaCommandHandler CreateHandler() => new(_currentUserService, _repository, _fileStorageService, _unitOfWork);
 
    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_CurrentUserIdIsNotAValidGuid()
    {
        _currentUserService.UserId.Returns((string?)null);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new DeleteMediaCommand(Guid.NewGuid()), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.NotAuthenticated");
    }
 
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_AssetDoesNotExistForThisOwner()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.UserId.Returns(ownerId.ToString());
        _repository.GetByIdForOwnerAsync(Arg.Any<Guid>(), ownerId, Arg.Any<CancellationToken>()).Returns((MediaAsset?)null);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new DeleteMediaCommand(Guid.NewGuid()), CancellationToken.None);
 
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Media.NotFound");
    }
 
    [Fact]
    public async Task Handle_Should_RemoveRowAndDeleteBothFiles_When_ThumbnailExists()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.UserId.Returns(ownerId.ToString());
 
        var asset = MediaAsset.Create(
            ownerId, MediaKind.Image, MediaCategory.User, "avatar.jpg",
            $"users/{ownerId:N}/x.jpg", $"thumbnails/x.jpg", "image/jpeg", 1024, 400, 400);
 
        _repository.GetByIdForOwnerAsync(asset.Id, ownerId, Arg.Any<CancellationToken>()).Returns(asset);
 
        var handler = CreateHandler();
        var result = await handler.Handle(new DeleteMediaCommand(asset.Id), CancellationToken.None);
 
        result.IsSuccess.Should().BeTrue();
        _repository.Received(1).Remove(asset);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _fileStorageService.Received(1).DeleteAsync(asset.StoragePath, Arg.Any<CancellationToken>());
        await _fileStorageService.Received(1).DeleteAsync(asset.ThumbnailStoragePath!, Arg.Any<CancellationToken>());
    }
 
    [Fact]
    public async Task Handle_Should_NotAttemptThumbnailDeletion_When_NoThumbnailExists()
    {
        var ownerId = Guid.NewGuid();
        _currentUserService.UserId.Returns(ownerId.ToString());
 
        var asset = MediaAsset.Create(
            ownerId, MediaKind.Other, MediaCategory.Message, "file.bin",
            $"messages/{ownerId:N}/x.bin", null, "application/octet-stream", 1024);
 
        _repository.GetByIdForOwnerAsync(asset.Id, ownerId, Arg.Any<CancellationToken>()).Returns(asset);
 
        var handler = CreateHandler();
        await handler.Handle(new DeleteMediaCommand(asset.Id), CancellationToken.None);
 
        await _fileStorageService.Received(1).DeleteAsync(asset.StoragePath, Arg.Any<CancellationToken>());
        await _fileStorageService.Received(1).DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}