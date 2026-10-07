using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

public sealed class FileReadDbModel : AuditableReadDbModel<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long? Size { get; set; }

    public FolderReadDbModel? Folder { get; set; }
}
