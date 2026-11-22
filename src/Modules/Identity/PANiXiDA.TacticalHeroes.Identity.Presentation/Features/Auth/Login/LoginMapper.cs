using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.GetAuthenticatedUser;
using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.Login;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Auth.Login;

[Mapper]
internal static partial class LoginMapper
{
    [MapperIgnoreSource(nameof(LoginRequest.ReturnUrl))]
    internal static partial LoginCommand ToCommand(LoginRequest request);

    [MapperIgnoreTarget(nameof(GetAuthenticatedUserQuery.AuthorizationId))]
    internal static partial GetAuthenticatedUserQuery ToQuery(LoginSecurityProof proof);
}
