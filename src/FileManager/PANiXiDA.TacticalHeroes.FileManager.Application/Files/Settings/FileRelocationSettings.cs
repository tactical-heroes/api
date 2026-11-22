namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Settings;

public sealed class FileRelocationSettings
{
    public TimeSpan CleanupDelay { get; init; } = TimeSpan.FromDays(7);
}
