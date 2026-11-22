using PANiXiDA.TacticalHeroes.Identity.Application.Users.Read.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Read.GetDetails;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class GetUserDetailsMapper
{
    internal static partial GetUserDetailsQuery ToQuery(Guid id);

    [MapperIgnoreSource(nameof(UserDetailsReadModel.IsBlocked))]
    internal static partial GetUserDetailsResponse ToResponse(UserDetailsReadModel user);
}
