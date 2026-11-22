using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.Create;
using PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Common;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Roles.Create;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class CreateRoleMapper
{
    internal static partial CreateRoleCommand ToCommand(CreateRoleRequest request);

    internal static partial CreateRoleResponse ToResponse(Guid id);
}
