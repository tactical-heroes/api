using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Create;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Create;

[Mapper]
internal static partial class CreateHeroMapper
{
    internal static partial CreateHeroCommand ToCommand(
        CreateHeroRequest request);

    internal static partial CreateHeroResponse ToResponse(Guid id);
}
