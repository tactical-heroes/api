using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetStatuses;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.GetStatuses;

[Mapper]
internal static partial class GetAccountStatusesMapper
{
    internal static partial IReadOnlyCollection<AccountStatusResponse> ToResponse(
        IReadOnlyCollection<AccountStatusReadModel> statuses);
}
