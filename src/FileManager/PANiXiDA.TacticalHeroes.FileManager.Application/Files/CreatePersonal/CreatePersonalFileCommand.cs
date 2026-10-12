namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonal;

public sealed record CreatePersonalFileCommand(
    string Name,
    string ContentType,
    long Size,
    Stream Content,
    Guid? FolderId) : ICommand<Result<Guid>>;
