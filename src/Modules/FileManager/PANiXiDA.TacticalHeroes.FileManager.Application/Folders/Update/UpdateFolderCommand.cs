namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Update;

public sealed record UpdateFolderCommand(
    Guid Id,
    string Name,
    string AllowedFileType,
    Guid? UserId = null,
    Guid? ParentId = null) : ICommand<Result>;
