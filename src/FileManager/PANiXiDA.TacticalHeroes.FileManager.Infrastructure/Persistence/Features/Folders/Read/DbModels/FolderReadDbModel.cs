using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;

public sealed class FolderReadDbModel : AuditableReadDbModel<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }

    public FolderReadDbModel? Parent { get; set; }
    public ICollection<FolderReadDbModel> Children { get; set; } = [];
    public ICollection<FileReadDbModel> Files { get; set; } = [];
}
