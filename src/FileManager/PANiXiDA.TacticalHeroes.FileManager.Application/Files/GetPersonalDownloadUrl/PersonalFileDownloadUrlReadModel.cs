namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDownloadUrl;

public sealed record PersonalFileDownloadUrlReadModel(
    string Url,
    DateTimeOffset ExpiresAt) : IReadModel;
