namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Write.Delete;

public sealed record DeleteFolderCommand(Guid Id) : ICommand<Result>;
