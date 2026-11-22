using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.MatchSearches.Cancel;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Presentation.Features.MatchSearches.Cancel;

[Mapper]
internal static partial class CancelMatchSearchMapper
{
    internal static partial CancelMatchSearchCommand ToCommand(Guid id);
}
