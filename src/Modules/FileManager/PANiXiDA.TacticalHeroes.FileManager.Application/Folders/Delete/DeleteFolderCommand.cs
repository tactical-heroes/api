namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Delete;

public sealed record DeleteFolderCommand(Guid Id) : ICommand<Result>;
