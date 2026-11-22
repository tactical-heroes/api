namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Move;

public sealed record MoveFileCommand(Guid Id, Guid? FolderId = null) : ICommand<Result>;
