namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreateUploadUrl;

public sealed record CreateFileUploadUrlCommand(
    string Name,
    string Type,
    string ContentType,
    long Size,
    Guid? UserId = null,
    Guid? FolderId = null) : ICommand<Result<FileUploadUrl>>;
