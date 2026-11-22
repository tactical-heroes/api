using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders.Events;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Events;

public sealed class FolderDeletedHandler(IFilesRepository filesRepository) : IEventHandler<FolderDeleted>
{
    public async Task HandleAsync(FolderDeleted domainEvent, CancellationToken cancellationToken)
    {
        var folderId = FolderId.Create(domainEvent.FolderId).Value;
        var files = await filesRepository.GetNonDeletingByFolderAsync(
            folderId: folderId,
            cancellationToken: cancellationToken);
        foreach (var file in files)
        {
            var result = file.BeginDeletion();
            if (result.IsSuccess)
            {
                filesRepository.Update(file);
            }
        }
    }
}
