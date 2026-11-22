using PANiXiDA.TacticalHeroes.Identity.Application.Users.Write.Update;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Write.Update;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class UpdateUserMapper
{
    internal static partial UpdateUserCommand ToCommand(
        UpdateUserRequest request,
        Guid id);
}
