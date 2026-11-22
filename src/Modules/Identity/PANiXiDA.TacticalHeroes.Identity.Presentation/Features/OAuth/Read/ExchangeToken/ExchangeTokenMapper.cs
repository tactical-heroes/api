using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetClientTokenPrincipal;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetUserTokenPrincipal;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Read.ExchangeToken;

[Mapper]
internal static partial class ExchangeTokenMapper
{
    internal static partial GetUserTokenPrincipalQuery ToUserQuery(Guid userId);

    internal static partial GetClientTokenPrincipalQuery ToClientQuery(
        string clientId);
}
