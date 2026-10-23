using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class UpdateUserMapper
{
    internal static partial UpdateUserCommand ToCommand(
        UpdateUserRequest request,
        Guid id);

    internal static ExchangeTokenQuery ToSessionQuery(Guid userId)
    {
        return new ExchangeTokenQuery(UserId: userId);
    }
}
