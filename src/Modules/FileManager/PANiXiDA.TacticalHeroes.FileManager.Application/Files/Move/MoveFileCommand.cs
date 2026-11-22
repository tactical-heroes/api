namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Move;

public sealed record MoveFileCommand(Guid Id, Guid? FolderId = null) : ICommand<Result>;
