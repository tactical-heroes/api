namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.CleanupVersion;

public sealed record CleanupFileVersionCommand(
    Guid FileId,
    string StorageKey,
    string VersionId) : ICommand<Result>;
