namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

internal sealed class FileUploadReconciliationOptions
{
    public const string SectionName = "FileManager:UploadReconciliation";

    public TimeSpan MinimumAge { get; init; } = TimeSpan.FromMinutes(15);
    public TimeSpan StorageRequestTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public int BatchSize { get; init; } = 100;
}
