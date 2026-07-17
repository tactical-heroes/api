using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ForgotPassword;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ForgotPassword;

[Mapper]
internal static partial class ForgotPasswordMapper
{
    internal static partial ForgotPasswordCommand ToCommand(ForgotPasswordRequest request);
}
