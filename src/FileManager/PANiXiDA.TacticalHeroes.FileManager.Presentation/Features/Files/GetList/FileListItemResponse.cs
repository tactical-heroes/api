namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetList;

public sealed record FileListItemResponse(
    Guid Id,
    string Name,
    string Type,
    Guid? UserId,
    Guid? FolderId,
    string Status,
    string? ContentType,
    long? Size,
    DateTime CreatedAt,
    DateTime UpdatedAt);
