namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDownloadUrl;

public sealed record PersonalFileDownloadReadModel(
    Guid Id,
    string Name,
    string StorageKey,
    string Status,
    string? ContentType) : IReadModel;
