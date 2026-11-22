namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders.Abstractions;

public interface IFoldersRepository : IRepository<FolderId, Folder>
{
    Task<List<Folder>> GetChildrenAsync(FolderId parentId, CancellationToken cancellationToken);
}
