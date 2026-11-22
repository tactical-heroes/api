using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ForgotPassword;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Write.ForgotPassword;

[Mapper]
internal static partial class ForgotPasswordMapper
{
    internal static partial ForgotPasswordCommand ToCommand(ForgotPasswordRequest request);
}
