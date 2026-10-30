namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.CreatePersonal;

public sealed record CreatePersonalFolderCommand(
    string Name,
    Guid? ParentId) : ICommand<Result<Guid>>;
