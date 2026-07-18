using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ConfirmEmail;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ConfirmEmail;

[Mapper]
internal static partial class ConfirmEmailMapper
{
    internal static partial ConfirmEmailCommand ToCommand(ConfirmEmailRequest request);
}
