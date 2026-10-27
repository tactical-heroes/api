namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

public sealed class FileUploadReconciliationOptions
{
    public const string SectionName = $"{nameof(FileManager)}:UploadReconciliation";

    public TimeSpan MinimumAge { get; init; } = TimeSpan.FromMinutes(15);
    public int BatchSize { get; init; } = 100;
}
