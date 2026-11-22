using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Start;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Start;

[Mapper]
internal static partial class StartMatchSearchMapper
{
    internal static partial StartMatchSearchCommand ToCommand(StartMatchSearchRequest request);
}
