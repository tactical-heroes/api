using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ChangePassword;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Write.ChangePassword;

[Mapper]
internal static partial class ChangePasswordMapper
{
    internal static partial ChangePasswordCommand ToCommand(ChangePasswordRequest request);
}
