namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Read.GetStatuses;

public sealed record GetUserStatusesQuery()
    : IQuery<Result<IReadOnlyCollection<UserStatusReadModel>>>;
