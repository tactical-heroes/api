namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Write.CreatePersonalUploadUrl;

public sealed record CreatePersonalFileUploadUrlResponse(
    Guid Id,
    string Url,
    DateTimeOffset ExpiresAt,
    IReadOnlyDictionary<string, string> RequiredHeaders);
