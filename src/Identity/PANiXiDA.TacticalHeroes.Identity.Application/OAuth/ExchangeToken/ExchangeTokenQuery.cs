namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;

public sealed record ExchangeTokenQuery(Guid AccountId)
    : IQuery<Result<ExchangeTokenReadModel>>;
