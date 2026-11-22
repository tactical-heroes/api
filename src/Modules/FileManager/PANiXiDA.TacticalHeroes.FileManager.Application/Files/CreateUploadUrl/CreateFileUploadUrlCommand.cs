using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Common.Models;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.CreateUploadUrl;

public sealed record CreateFileUploadUrlCommand(
    string Name,
    string Type,
    string ContentType,
    long Size,
    Guid? UserId = null,
    Guid? FolderId = null) : ICommand<Result<FileUploadUrl>>;
