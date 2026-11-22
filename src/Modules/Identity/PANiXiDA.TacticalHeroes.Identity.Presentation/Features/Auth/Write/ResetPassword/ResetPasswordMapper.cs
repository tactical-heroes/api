using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ResetPassword;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Write.ResetPassword;

[Mapper]
internal static partial class ResetPasswordMapper
{
    internal static partial ResetPasswordCommand ToCommand(ResetPasswordRequest request);
}
