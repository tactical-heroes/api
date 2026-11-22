using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;

public sealed class FolderAccessService(
    IFoldersRepository foldersRepository,
    IFolderHierarchyLock folderHierarchyLock,
    FolderAccessPolicy folderAccessPolicy)
{
    public Task<Result<Folder>> GetForCreateAsync(FolderId id, CancellationToken cancellationToken)
    {
        return GetAsync(
            id: id,
            canAccess: folderAccessPolicy.CanCreate,
            cancellationToken: cancellationToken);
    }

    public Task<Result<Folder>> GetForUpdateAsync(FolderId id, CancellationToken cancellationToken)
    {
        return GetAsync(
            id: id,
            canAccess: folderAccessPolicy.CanUpdate,
            cancellationToken: cancellationToken);
    }

    private async Task<Result<Folder>> GetAsync(
        FolderId id,
        Func<Folder, bool> canAccess,
        CancellationToken cancellationToken)
    {
        await folderHierarchyLock.AcquireAsync(cancellationToken);
        var folder = await foldersRepository.GetByIdAsync(id, cancellationToken);

        return folder is not null && canAccess(folder)
            ? Result.Success(folder)
            : Result.Failure<Folder>(Error.NotFound("Folder was not found."));
    }
}
