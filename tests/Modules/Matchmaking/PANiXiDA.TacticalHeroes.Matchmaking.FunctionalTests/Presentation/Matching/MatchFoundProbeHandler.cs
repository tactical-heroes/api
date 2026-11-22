using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Contracts.MatchSearches;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.FunctionalTests.Presentation.Matching;

public static class MatchFoundProbeHandler
{
    public static void Handle(MatchFound matchFound, MatchFoundProbe probe)
    {
        probe.Record(matchFound);
    }
}
