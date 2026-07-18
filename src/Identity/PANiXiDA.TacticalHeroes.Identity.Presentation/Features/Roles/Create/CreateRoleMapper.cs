using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class CreateRoleMapper
{
    internal static partial CreateRoleCommand ToCommand(CreateRoleRequest request);

    internal static partial CreateRoleResponse ToResponse(Guid id);
}
