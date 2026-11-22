namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.GetList;

public sealed record FolderListItemReadModel(
    Guid Id,
    string Name,
    string AllowedFileType,
    Guid? ParentId,
    string? ParentName) : IReadModel;
