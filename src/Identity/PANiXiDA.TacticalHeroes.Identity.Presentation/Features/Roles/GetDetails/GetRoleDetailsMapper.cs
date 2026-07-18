using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetDetails;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class GetRoleDetailsMapper
{
    internal static partial GetRoleDetailsResponse ToResponse(RoleDetailsReadModel role);
}
