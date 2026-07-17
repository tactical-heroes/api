using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Update;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.Update;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class UpdateAccountMapper
{
    internal static partial UpdateAccountCommand ToCommand(
        UpdateAccountRequest request,
        Guid id);
}
