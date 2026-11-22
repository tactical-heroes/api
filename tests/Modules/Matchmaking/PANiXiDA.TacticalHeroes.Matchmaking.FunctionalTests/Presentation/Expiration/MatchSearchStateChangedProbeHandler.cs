using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Expiration;

public static class MatchSearchStateChangedProbeHandler
{
    public static void Handle(MatchSearchStateChanged domainEvent, MatchSearchStateChangedProbe probe)
    {
        probe.Record(domainEvent);
    }
}
