namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.ExchangeToken;

public sealed record ExchangeUserTokenQuery(Guid UserId)
    : IQuery<Result<ExchangeTokenReadModel>>;
