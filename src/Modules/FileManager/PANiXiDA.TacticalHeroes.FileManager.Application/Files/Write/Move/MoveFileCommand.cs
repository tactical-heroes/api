namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.Move;

public sealed record MoveFileCommand(Guid Id, Guid? FolderId = null) : ICommand<Result>;
