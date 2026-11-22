namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Write.CreatePersonal;

public sealed record CreatePersonalFolderCommand(
    string Name,
    Guid? ParentId) : ICommand<Result<Guid>>;
