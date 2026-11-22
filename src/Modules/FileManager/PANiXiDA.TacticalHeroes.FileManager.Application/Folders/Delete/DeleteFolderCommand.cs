namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Delete;

public sealed record DeleteFolderCommand(Guid Id) : ICommand<Result>;
