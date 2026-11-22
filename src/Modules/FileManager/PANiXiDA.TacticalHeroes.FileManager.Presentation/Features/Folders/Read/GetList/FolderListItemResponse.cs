namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Read.GetList;

public sealed record FolderListItemResponse(
    Guid Id,
    string Name,
    string AllowedFileType,
    Guid? ParentId,
    string? ParentName);
