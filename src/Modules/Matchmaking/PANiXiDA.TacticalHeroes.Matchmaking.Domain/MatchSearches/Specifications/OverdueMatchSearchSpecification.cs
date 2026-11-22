using System.Linq.Expressions;

using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.Specifications;

public sealed class OverdueMatchSearchSpecification(MatchmakingTimestamp now) : Specification<MatchSearch>
{
    public override Expression<Func<MatchSearch, bool>> ToExpression()
    {
        return search => search.Status == MatchSearchStatus.Searching && search.ExpiresAt <= now;
    }
}
