namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;

public sealed record ExchangeUserTokenQuery(Guid UserId)
    : IQuery<Result<ExchangeTokenReadModel>>;
