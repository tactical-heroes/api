using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.GetStatuses;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Users.GetStatuses;

[Mapper]
internal static partial class GetUserStatusesMapper
{
    internal static GetUserStatusesQuery ToQuery() => new();

    internal static partial IReadOnlyCollection<UserStatusResponse> ToResponse(
        IReadOnlyCollection<UserStatusReadModel> statuses);
}
