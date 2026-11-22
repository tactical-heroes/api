using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Read.GetAuthenticatedUser;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.Login;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Write.Login;

[Mapper]
internal static partial class LoginMapper
{
    [MapperIgnoreSource(nameof(LoginRequest.ReturnUrl))]
    internal static partial LoginCommand ToCommand(LoginRequest request);

    [MapperIgnoreTarget(nameof(GetAuthenticatedUserQuery.AuthorizationId))]
    internal static partial GetAuthenticatedUserQuery ToQuery(LoginSecurityProof proof);
}
