namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Abstractions;

public interface IFolderHierarchyLock
{
    Task AcquireAsync(CancellationToken cancellationToken);
}
