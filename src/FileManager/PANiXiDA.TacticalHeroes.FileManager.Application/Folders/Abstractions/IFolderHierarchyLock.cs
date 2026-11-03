namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;

public interface IFolderHierarchyLock
{
    /// <summary>
    /// Acquires exclusive access to hierarchy changes until the surrounding transaction completes.
    /// </summary>
    Task AcquireAsync(CancellationToken cancellationToken);
}
