namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Options.FileUploadReconciliation;

public sealed class FileUploadReconciliationOptions
{
    public const string SectionName = "FileManager:UploadReconciliation";

    public TimeSpan MinimumAge { get; init; } = TimeSpan.FromMinutes(15);
    public TimeSpan StorageRequestTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public int BatchSize { get; init; } = 100;
}
