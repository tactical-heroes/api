namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonalUploadUrl;

public sealed record CreatePersonalFileUploadUrlRequest(
    string Name,
    string ContentType,
    long Size,
    Guid? FolderId = null);
