using PANiXiDA.TacticalHeroes.Identity.Application.Users.Write.Block;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Write.Block;

[Mapper]
internal static partial class BlockUserMapper
{
    internal static partial BlockUserCommand ToCommand(Guid id);
}
