namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Update;

public sealed record UpdateFileCommand(
    Guid Id,
    string Name,
    string Type,
    Guid? UserId = null,
    Guid? FolderId = null) : ICommand<Result>;
