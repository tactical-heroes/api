namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetList;

public sealed record FolderListItemResponse(
    Guid Id,
    string Name,
    string AllowedFileType,
    Guid? ParentId,
    string? ParentName);
