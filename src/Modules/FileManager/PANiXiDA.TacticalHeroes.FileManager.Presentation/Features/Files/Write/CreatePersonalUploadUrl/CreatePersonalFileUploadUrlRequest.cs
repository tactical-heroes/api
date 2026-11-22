namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Write.CreatePersonalUploadUrl;

public sealed record CreatePersonalFileUploadUrlRequest(
    string Name,
    string ContentType,
    long Size,
    Guid? FolderId = null);
