using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Update;

[Mapper]
internal static partial class UpdateFactionMapper
{
    internal static partial UpdateFactionCommand ToCommand(
        UpdateFactionRequest request,
        Guid id);
}
