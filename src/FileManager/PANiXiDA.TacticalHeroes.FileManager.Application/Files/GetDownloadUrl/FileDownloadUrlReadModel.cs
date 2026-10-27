namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDownloadUrl;

public sealed record FileDownloadUrlReadModel(
    string Url,
    DateTimeOffset ExpiresAt) : IReadModel;
