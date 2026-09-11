using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ChangePassword;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ChangePassword;

[Mapper]
internal static partial class ChangePasswordMapper
{
    internal static partial ChangePasswordCommand ToCommand(
        ChangePasswordRequest request,
        Guid userId);
}
