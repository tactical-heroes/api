using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Write.Start;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Write.Start;

[Mapper]
internal static partial class StartMatchSearchMapper
{
    internal static partial StartMatchSearchCommand ToCommand(StartMatchSearchRequest request);
}
