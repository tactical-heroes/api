namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Read.GetList;

public sealed record FolderListItemReadModel(
    Guid Id,
    string Name,
    string AllowedFileType,
    Guid? ParentId,
    string? ParentName) : IReadModel;
