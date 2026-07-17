using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.GetDetails;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class GetAccountDetailsMapper
{
    internal static partial GetAccountDetailsResponse ToResponse(AccountDetailsReadModel account);
}
