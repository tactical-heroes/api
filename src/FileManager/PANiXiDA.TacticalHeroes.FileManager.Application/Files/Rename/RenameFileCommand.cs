namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Rename;

public sealed record RenameFileCommand(Guid Id, string Name) : ICommand<Result>;
