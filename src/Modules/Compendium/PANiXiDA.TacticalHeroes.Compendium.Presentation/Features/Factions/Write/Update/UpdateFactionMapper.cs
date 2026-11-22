using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Write.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Write.Update;

[Mapper]
internal static partial class UpdateFactionMapper
{
    internal static partial UpdateFactionCommand ToCommand(
        UpdateFactionRequest request,
        Guid id);
}
