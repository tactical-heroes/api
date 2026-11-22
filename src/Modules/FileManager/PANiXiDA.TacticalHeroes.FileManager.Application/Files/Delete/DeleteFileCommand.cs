namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Delete;

public sealed record DeleteFileCommand(Guid Id) : ICommand<Result>;
