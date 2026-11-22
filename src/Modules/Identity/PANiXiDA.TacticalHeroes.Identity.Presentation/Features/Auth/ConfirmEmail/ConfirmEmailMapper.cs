using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.ConfirmEmail;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Auth.ConfirmEmail;

[Mapper]
internal static partial class ConfirmEmailMapper
{
    internal static partial ConfirmEmailCommand ToCommand(ConfirmEmailRequest request);
}
