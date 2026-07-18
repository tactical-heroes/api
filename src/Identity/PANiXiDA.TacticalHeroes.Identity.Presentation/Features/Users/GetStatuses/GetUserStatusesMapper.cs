using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetStatuses;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetStatuses;

[Mapper]
internal static partial class GetUserStatusesMapper
{
    internal static partial IReadOnlyCollection<UserStatusResponse> ToResponse(
        IReadOnlyCollection<UserStatusReadModel> statuses);
}
