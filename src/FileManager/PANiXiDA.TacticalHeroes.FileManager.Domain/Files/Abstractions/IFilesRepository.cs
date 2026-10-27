namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;

public interface IFilesRepository : IRepository<FileId, File>
{
    IAsyncEnumerable<File> GetPendingUploadsAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetExpiredUploadsAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetFilesForDeletionAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetDeletedFilesAsync(CancellationToken cancellationToken);
}
