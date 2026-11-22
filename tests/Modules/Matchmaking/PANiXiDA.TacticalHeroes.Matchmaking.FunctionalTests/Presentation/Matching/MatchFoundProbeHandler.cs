using PANiXiDA.TacticalHeroes.Matchmaking.Contracts.MatchSearches;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;

public static class MatchFoundProbeHandler
{
    public static void Handle(MatchFound matchFound, MatchFoundProbe probe)
    {
        probe.Record(matchFound);
    }
}
