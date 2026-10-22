using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Login;

[Mapper]
internal static partial class LoginMapper
{
    [MapperIgnoreSource(nameof(LoginRequest.ReturnUrl))]
    internal static partial LoginCommand ToCommand(LoginRequest request);
}
