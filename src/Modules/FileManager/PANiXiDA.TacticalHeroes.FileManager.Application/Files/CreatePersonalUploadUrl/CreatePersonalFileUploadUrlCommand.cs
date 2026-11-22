using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Models;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonalUploadUrl;

public sealed record CreatePersonalFileUploadUrlCommand(
    string Name,
    string ContentType,
    long Size,
    Guid? FolderId = null) : ICommand<Result<FileUploadUrl>>;
