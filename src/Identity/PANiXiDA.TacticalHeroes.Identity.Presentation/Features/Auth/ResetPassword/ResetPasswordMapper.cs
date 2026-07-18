using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResetPassword;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResetPassword;

[Mapper]
internal static partial class ResetPasswordMapper
{
    internal static partial ResetPasswordCommand ToCommand(ResetPasswordRequest request);
}
