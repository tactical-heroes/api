namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;

public interface IFoldersRepository : IRepository<FolderId, Folder>
{
    Task<Folder?> GetUntrackedByIdAsync(FolderId id, CancellationToken cancellationToken);
    Task<List<Folder>> GetChildrenAsync(FolderId parentId, CancellationToken cancellationToken);
}
