using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Write.Create;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Write.Create;

[Mapper]
internal static partial class CreateHeroMapper
{
    internal static partial CreateHeroCommand ToCommand(
        CreateHeroRequest request);

    internal static partial CreateHeroResponse ToResponse(Guid id);
}
