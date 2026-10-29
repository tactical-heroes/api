namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling.Options.MatchmakingProcessing;

public sealed class MatchmakingProcessingOptions
{
    public const string SectionName = $"{nameof(Matchmaking)}:Processing";

    public bool Enabled { get; init; }
    public TimeSpan Interval { get; init; } = TimeSpan.FromSeconds(1);
}
