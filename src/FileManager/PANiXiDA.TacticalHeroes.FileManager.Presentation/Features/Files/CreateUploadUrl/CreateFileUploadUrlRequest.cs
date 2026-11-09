namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreateUploadUrl;

public sealed record CreateFileUploadUrlRequest(
    string Name,
    string Type,
    string ContentType,
    long Size,
    Guid? UserId = null,
    Guid? FolderId = null);
