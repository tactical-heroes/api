using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ResendConfirmationEmail;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Write.ResendConfirmationEmail;

[Mapper]
internal static partial class ResendConfirmationEmailMapper
{
    internal static partial ResendConfirmationEmailCommand ToCommand(
        ResendConfirmationEmailRequest request);
}
