using PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.ExchangeToken;
using PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.GetClientTokenPrincipal;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.OAuth.ExchangeToken;

[Mapper]
internal static partial class ExchangeTokenMapper
{
    internal static partial ExchangeTokenQuery ToUserQuery(Guid userId);

    internal static partial GetClientTokenPrincipalQuery ToClientQuery(
        string clientId);
}
