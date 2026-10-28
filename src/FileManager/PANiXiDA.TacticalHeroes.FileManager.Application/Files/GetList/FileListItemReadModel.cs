namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;

public sealed record FileListItemReadModel(
    Guid Id,
    string Name,
    string Type,
    Guid? UserId,
    Guid? FolderId,
    string Status,
    string? ContentType,
    long? Size,
    DateTime CreatedAt,
    DateTime UpdatedAt) : IReadModel;
