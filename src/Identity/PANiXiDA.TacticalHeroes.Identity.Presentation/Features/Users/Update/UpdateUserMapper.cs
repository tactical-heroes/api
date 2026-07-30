using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class UpdateUserMapper
{
    internal static partial UpdateUserCommand ToCommand(
        UpdateUserRequest request,
        Guid id);
}
