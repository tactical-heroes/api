namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Create;

public sealed record CreateFolderCommand(
    string Name,
    string AllowedFileType,
    Guid? UserId,
    Guid? ParentId) : ICommand<Result<Guid>>;
