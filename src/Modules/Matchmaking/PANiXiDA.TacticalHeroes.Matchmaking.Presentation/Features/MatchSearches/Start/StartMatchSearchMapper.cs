using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Start;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Presentation.Features.MatchSearches.Start;

[Mapper]
internal static partial class StartMatchSearchMapper
{
    internal static partial StartMatchSearchCommand ToCommand(StartMatchSearchRequest request);
}
