namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Options;

public sealed class FileRelocationOptions
{
    public const string SectionName = $"{nameof(FileManager)}:Relocation";

    public TimeSpan CleanupDelay { get; init; } = TimeSpan.FromDays(7);
}
