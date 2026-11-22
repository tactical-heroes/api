using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Write.Cancel;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Write.Cancel;

[Mapper]
internal static partial class CancelMatchSearchMapper
{
    internal static partial CancelMatchSearchCommand ToCommand(Guid id);
}
