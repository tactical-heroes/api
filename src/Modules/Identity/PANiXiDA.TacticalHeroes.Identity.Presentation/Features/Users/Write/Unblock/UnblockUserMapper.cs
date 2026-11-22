using PANiXiDA.TacticalHeroes.Identity.Application.Users.Write.Unblock;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Write.Unblock;

[Mapper]
internal static partial class UnblockUserMapper
{
    internal static partial UnblockUserCommand ToCommand(Guid id);
}
