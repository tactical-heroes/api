using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Write.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Write.Delete;

[Mapper]
internal static partial class DeleteFactionMapper
{
    internal static partial DeleteFactionCommand ToCommand(Guid id);
}
