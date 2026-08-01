using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetClientTokenPrincipal;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.ExchangeToken;

[Mapper]
internal static partial class ExchangeTokenMapper
{
    internal static partial ExchangeTokenQuery ToUserQuery(Guid userId);

    internal static partial GetClientTokenPrincipalQuery ToClientQuery(
        string clientId);

    internal static IReadOnlyCollection<Claim> ToClaims(
        ExchangeTokenReadModel readModel)
    {
        return readModel.Claims;
    }

    internal static IReadOnlyCollection<Claim> ToClaims(
        OAuthClientTokenPrincipalReadModel readModel)
    {
        return readModel.Claims;
    }
}
