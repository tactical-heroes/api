namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonalUploadUrl;

public sealed record CreatePersonalFileUploadUrlResponse(
    Guid Id,
    string Url,
    DateTimeOffset ExpiresAt,
    IReadOnlyDictionary<string, string> RequiredHeaders);
