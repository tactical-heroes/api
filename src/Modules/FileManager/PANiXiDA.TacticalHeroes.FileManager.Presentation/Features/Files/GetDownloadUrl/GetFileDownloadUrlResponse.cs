namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.GetDownloadUrl;

public sealed record GetFileDownloadUrlResponse(
    string Url,
    DateTimeOffset ExpiresAt);
