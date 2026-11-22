namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Move;

public sealed record MoveFolderCommand(Guid Id, Guid? ParentId = null) : ICommand<Result>;
