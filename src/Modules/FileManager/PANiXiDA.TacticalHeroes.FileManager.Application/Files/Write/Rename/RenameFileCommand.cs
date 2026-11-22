namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.Rename;

public sealed record RenameFileCommand(Guid Id, string Name) : ICommand<Result>;
