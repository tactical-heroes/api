namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDetails;

public sealed record FileDetailsReadModel(
    Guid Id,
    string Name,
    string Type,
    Guid? UserId,
    Guid? FolderId,
    string Status,
    string? ContentType,
    long? Size) : IReadModel;
