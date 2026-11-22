namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Deferred.CleanupVersion;

public sealed record CleanupFileVersionCommand(
    Guid FileId,
    string StorageKey,
    string VersionId) : ICommand<Result>;
