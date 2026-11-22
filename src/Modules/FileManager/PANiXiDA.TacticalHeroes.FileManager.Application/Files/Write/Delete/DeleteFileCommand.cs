namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.Delete;

public sealed record DeleteFileCommand(Guid Id) : ICommand<Result>;
