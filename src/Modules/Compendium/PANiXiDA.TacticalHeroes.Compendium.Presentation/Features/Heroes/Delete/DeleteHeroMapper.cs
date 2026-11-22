using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Delete;

[Mapper]
internal static partial class DeleteHeroMapper
{
    internal static partial DeleteHeroCommand ToCommand(Guid id);
}
