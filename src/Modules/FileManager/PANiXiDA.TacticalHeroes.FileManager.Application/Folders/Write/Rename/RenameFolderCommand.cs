namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Write.Rename;

public sealed record RenameFolderCommand(Guid Id, string Name) : ICommand<Result>;
