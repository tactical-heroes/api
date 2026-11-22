using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.Abstractions;

public interface IFilesRepository : IRepository<FileId, File>
{
    Task<List<File>> GetNonDeletingByFolderAsync(FolderId folderId, CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetRelocatingFilesAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetPendingUploadsAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetUpdatingFilesAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetExpiredContentUpdatesAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetExpiredUploadsAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetFilesForDeletionAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetDeletedFilesAsync(CancellationToken cancellationToken);
}
