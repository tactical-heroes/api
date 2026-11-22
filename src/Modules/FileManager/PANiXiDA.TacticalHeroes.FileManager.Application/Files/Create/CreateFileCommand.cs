namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Create;

public sealed record CreateFileCommand(
    string Name,
    string Type,
    string ContentType,
    long Size,
    Stream Content,
    Guid? UserId = null,
    Guid? FolderId = null) : ICommand<Result<Guid>>;
