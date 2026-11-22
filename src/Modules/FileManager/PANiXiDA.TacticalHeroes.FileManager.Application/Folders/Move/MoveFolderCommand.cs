namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Move;

public sealed record MoveFolderCommand(Guid Id, Guid? ParentId = null) : ICommand<Result>;
