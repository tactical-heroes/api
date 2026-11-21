namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CleanupVersion;

public sealed record CleanupFileVersionCommand(
    Guid FileId,
    string StorageKey,
    string VersionId) : ICommand<Result>;
