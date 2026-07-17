using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.Create;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class CreateAccountMapper
{
    internal static partial CreateAccountCommand ToCommand(CreateAccountRequest request);

    internal static partial CreateAccountResponse ToResponse(Guid id);
}
