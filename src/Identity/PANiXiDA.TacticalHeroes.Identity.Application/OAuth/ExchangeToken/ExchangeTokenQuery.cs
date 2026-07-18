namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;

public sealed record ExchangeTokenQuery(Guid UserId)
    : IQuery<Result<ExchangeTokenReadModel>>;
