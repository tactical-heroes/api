namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.GetStatuses;

public sealed record GetUserStatusesQuery()
    : IQuery<Result<IReadOnlyCollection<UserStatusReadModel>>>;
