namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;

public interface IFoldersRepository : IRepository<FolderId, Folder>
{
    Task LockHierarchyAsync(CancellationToken cancellationToken);
    Task<List<Folder>> GetChildrenAsync(FolderId parentId, CancellationToken cancellationToken);
}
