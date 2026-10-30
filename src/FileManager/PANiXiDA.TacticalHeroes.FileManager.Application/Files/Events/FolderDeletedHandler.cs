using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Events;

public sealed class FolderDeletedHandler(IFilesRepository filesRepository) : IEventHandler<FolderDeleted>
{
    public async Task HandleAsync(FolderDeleted domainEvent, CancellationToken cancellationToken)
    {
        var folderId = FolderId.Create(domainEvent.FolderId).Value;
        var files = await filesRepository.GetByFolderForDeletionAsync(
            folderId: folderId,
            cancellationToken: cancellationToken);
        foreach (var file in files)
        {
            file.BeginDeletion();
            filesRepository.Update(file);
        }
    }
}
