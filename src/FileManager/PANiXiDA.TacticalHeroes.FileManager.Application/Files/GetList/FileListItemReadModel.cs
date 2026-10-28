namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;

public sealed record FileListItemReadModel(
    Guid Id,
    string Name,
    string Status,
    string? ContentType,
    long? Size) : IReadModel;
