namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;

public interface IFilesRepository : IRepository<FileId, File>
{
    IAsyncEnumerable<File> GetPendingPersonalUploadsAsync(CancellationToken cancellationToken);
}
