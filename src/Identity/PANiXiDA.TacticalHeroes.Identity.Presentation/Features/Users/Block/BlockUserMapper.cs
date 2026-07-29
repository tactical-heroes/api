using PANiXiDA.TacticalHeroes.Identity.Application.Users.Block;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Block;

[Mapper]
internal static partial class BlockUserMapper
{
    internal static partial BlockUserCommand ToCommand(Guid id);
}
