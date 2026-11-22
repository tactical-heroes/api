using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Factions.Delete;

[Mapper]
internal static partial class DeleteFactionMapper
{
    internal static partial DeleteFactionCommand ToCommand(Guid id);
}
