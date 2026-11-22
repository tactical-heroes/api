namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.UpdateContent;

public sealed record UpdateFileContentCommand(
    Guid Id,
    string ContentType,
    long Size,
    Stream Content) : ICommand<Result>;
