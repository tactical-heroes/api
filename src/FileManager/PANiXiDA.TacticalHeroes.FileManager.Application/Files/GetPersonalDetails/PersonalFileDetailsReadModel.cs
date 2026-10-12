namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;

public sealed record PersonalFileDetailsReadModel(
    Guid Id,
    string Name,
    Guid? FolderId,
    string Status,
    string? ContentType,
    long? Size) : IReadModel;
