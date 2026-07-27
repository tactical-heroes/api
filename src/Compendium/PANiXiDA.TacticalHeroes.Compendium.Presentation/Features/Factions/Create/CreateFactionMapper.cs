using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Create;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.Create;

[Mapper]
internal static partial class CreateFactionMapper
{
    internal static partial CreateFactionCommand ToCommand(
        CreateFactionRequest request);

    internal static partial CreateFactionResponse ToResponse(Guid id);
}
