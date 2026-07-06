using MediatR;
using SocialHub.Application.Common.Interfaces;
using SocialHub.Application.Common.Results;
 
namespace SocialHub.Application.Features.Media;
 
// Plain IRequestHandler<TCommand, Result>, matching LogoutCommandHandler's
// pattern for void-returning commands — there is no non-generic
// ICommandHandler<TCommand> wrapper in this codebase (only the
// TResponse-returning ICommandHandler<TCommand, TResponse> exists).
public sealed class DeleteMediaCommandHandler : IRequestHandler<DeleteMediaCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediaAssetRepository _repository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
 
    public DeleteMediaCommandHandler(
        ICurrentUserService currentUserService,
        IMediaAssetRepository repository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _repository = repository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }
 
    public async Task<Result> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var ownerId))
        {
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication is required."));
        }
 
        var asset = await _repository.GetByIdForOwnerAsync(request.MediaId, ownerId, cancellationToken);
        if (asset is null)
        {
            return Result.Failure(Error.NotFound("Media.NotFound", "Media not found."));
        }
 
        asset.MarkDeleted();
        _repository.Remove(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        // Files are removed only after the DB row is confirmed gone —
        // mirrors the upload path's "DB is the source of truth" discipline
        // (spec §22). If file deletion itself fails partway, the row is
        // already gone and the leftover file is a plain orphan the way any
        // manually-deleted-from-disk file would be; no automated sweep for
        // that specific case exists yet (only temp/ is swept, per 4.7).
        await _fileStorageService.DeleteAsync(asset.StoragePath, cancellationToken);
        if (asset.ThumbnailStoragePath is not null)
        {
            await _fileStorageService.DeleteAsync(asset.ThumbnailStoragePath, cancellationToken);
        }
 
        return Result.Success();
    }
}