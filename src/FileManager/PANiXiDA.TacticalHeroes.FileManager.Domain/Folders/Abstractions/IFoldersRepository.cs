namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;

public interface IFoldersRepository : IRepository<FolderId, Folder>
{
    Task<List<Folder>> GetChildrenAsync(FolderId parentId, CancellationToken cancellationToken);
    IAsyncEnumerable<Folder> GetDeletedWithActiveContentsAsync(CancellationToken cancellationToken);
}
