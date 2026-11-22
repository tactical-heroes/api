using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Heroes.Create;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Heroes.Create;

[Mapper]
internal static partial class CreateHeroMapper
{
    internal static partial CreateHeroCommand ToCommand(
        CreateHeroRequest request);

    internal static partial CreateHeroResponse ToResponse(Guid id);
}
