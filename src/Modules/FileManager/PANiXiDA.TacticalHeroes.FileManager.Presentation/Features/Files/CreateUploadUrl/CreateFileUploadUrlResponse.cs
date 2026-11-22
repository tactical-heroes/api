namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.CreateUploadUrl;

public sealed record CreateFileUploadUrlResponse(
    Guid Id,
    string Url,
    DateTimeOffset ExpiresAt,
    IReadOnlyDictionary<string, string> RequiredHeaders);
