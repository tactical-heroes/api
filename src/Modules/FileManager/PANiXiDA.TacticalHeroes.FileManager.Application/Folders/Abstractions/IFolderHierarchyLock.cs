namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;

public interface IFolderHierarchyLock
{
    Task AcquireAsync(CancellationToken cancellationToken);
}
