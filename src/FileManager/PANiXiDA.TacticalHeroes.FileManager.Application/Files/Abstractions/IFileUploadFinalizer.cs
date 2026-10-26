using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

public interface IFileUploadFinalizer
{
    Task<Result> CompleteAsync(
        FileId id,
        FileContentType contentType,
        FileSize size,
        CancellationToken cancellationToken);
}
