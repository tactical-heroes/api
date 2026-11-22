using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Factions.Update;

[Mapper]
internal static partial class UpdateFactionMapper
{
    internal static partial UpdateFactionCommand ToCommand(
        UpdateFactionRequest request,
        Guid id);
}
