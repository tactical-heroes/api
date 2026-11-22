namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Read.GetDownloadUrl;

public sealed record GetFileDownloadUrlResponse(
    string Url,
    DateTimeOffset ExpiresAt);
