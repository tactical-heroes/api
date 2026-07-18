namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetStatuses;

public sealed record GetUserStatusesQuery()
    : IQuery<Result<IReadOnlyCollection<UserStatusReadModel>>>;
