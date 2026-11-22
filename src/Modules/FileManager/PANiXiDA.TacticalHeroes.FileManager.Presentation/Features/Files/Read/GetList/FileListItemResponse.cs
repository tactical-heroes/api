namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Read.GetList;

public sealed record FileListItemResponse(
    Guid Id,
    string Name,
    string Type,
    string Status,
    string? ContentType,
    long? Size);
