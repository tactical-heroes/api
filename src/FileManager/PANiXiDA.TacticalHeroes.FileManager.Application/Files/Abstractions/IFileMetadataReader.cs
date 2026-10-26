using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Storage;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

public interface IFileMetadataReader
{
    Task<StoredFileMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken);
}
