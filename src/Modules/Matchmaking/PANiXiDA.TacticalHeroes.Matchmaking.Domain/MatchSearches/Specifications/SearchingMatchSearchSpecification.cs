using System.Linq.Expressions;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Specifications;

public sealed class SearchingMatchSearchSpecification(MatchmakingTimestamp now) : Specification<MatchSearch>
{
    public override Expression<Func<MatchSearch, bool>> ToExpression()
    {
        return search => search.Status == MatchSearchStatus.Searching
            && search.QueuedAt <= now
            && search.ExpiresAt > now;
    }
}
