namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Write.Move;

public sealed record MoveFolderCommand(Guid Id, Guid? ParentId = null) : ICommand<Result>;
