using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ResendConfirmationEmail;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ResendConfirmationEmail;

[Mapper]
internal static partial class ResendConfirmationEmailMapper
{
    internal static partial ResendConfirmationEmailCommand ToCommand(
        ResendConfirmationEmailRequest request);
}
