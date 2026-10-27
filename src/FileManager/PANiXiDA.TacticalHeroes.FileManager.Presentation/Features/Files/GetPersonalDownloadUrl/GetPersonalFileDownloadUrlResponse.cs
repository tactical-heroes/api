namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetPersonalDownloadUrl;

public sealed record GetPersonalFileDownloadUrlResponse(
    string Url,
    DateTimeOffset ExpiresAt);
