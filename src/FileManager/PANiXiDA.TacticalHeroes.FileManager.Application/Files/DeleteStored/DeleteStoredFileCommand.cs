namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.DeleteStored;

public sealed record DeleteStoredFileCommand(
    string StorageKey,
    DateTimeOffset RecheckUntil) : ICommand<Result>;
