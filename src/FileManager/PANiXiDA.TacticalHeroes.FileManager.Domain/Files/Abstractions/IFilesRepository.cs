namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;

public interface IFilesRepository : IRepository<FileId, File>
{
    IAsyncEnumerable<File> GetPendingPersonalUploadsAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetExpiredPersonalUploadsAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetPersonalFilesForDeletionAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<File> GetDeletedPersonalFilesAsync(CancellationToken cancellationToken);
}
