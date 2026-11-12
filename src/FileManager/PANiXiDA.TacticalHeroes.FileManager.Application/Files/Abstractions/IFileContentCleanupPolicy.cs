using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

public interface IFileContentCleanupPolicy
{
    bool CanBeginDeletion(FileContent content);
    bool CanForget(FileContent content);
}
