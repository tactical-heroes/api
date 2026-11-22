using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetStatuses;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetStatuses;

[Mapper]
internal static partial class GetUserStatusesMapper
{
    internal static GetUserStatusesQuery ToQuery() => new();

    internal static partial IReadOnlyCollection<UserStatusResponse> ToResponse(
        IReadOnlyCollection<UserStatusReadModel> statuses);
}
