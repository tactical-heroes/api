namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Rename;

public sealed record RenameFolderCommand(Guid Id, string Name) : ICommand<Result>;
