namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Delete;

public sealed record DeleteFileCommand(Guid Id) : ICommand<Result>;
